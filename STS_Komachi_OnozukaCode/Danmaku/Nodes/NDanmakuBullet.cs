using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.TestSupport;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Danmaku.Nodes
{
    public partial class NDanmakuBullet : Sprite2D
    {
        public float Speed;
        public float AngleRad;
        public float AngleDeg { get => Mathf.RadToDeg(AngleRad); set => AngleRad = Mathf.DegToRad(value); }
        public float Acceleration;
        public float AccelerationAngleDeg;
        public Creature? HomingTargetOverride;
        public float ElapsedLifetime => _elapsed;

        float _elapsed;
        float _lifetimeSeconds;
        IReadOnlyList<Creature> _targets;
        Action? _onHit;
        List<DanmakuEvent> _events = new(); 
        Line2D? _trail; 
        List<(Vector2 Pos, float Time)> _trailPoints = new();
        // how long a point lives before fading out of the trail
        const float TrailMaxAge = 0.25f; 
        bool _spawnShards;

        int _hitsRemaining;
        bool _zeroHitNotDie;
        float _hitIntervalSeconds;
        float _lastHitTime = float.NegativeInfinity; 
        string? _onHitSfx;
        bool HitsExhausted => _hitsRemaining <= 0;
        // Spawning Fields
        bool _entering;
        float _enteringElapsed;
        Vector2 _entryTargetScale;
        const float EntryGrowDurationSeconds = 0.2f;
        // Dying fields
        bool _dying;
        float _dyingElapsed;
        Vector2 _deathScale;
        static float ExitFadeDurationSeconds => 0.2f;

        public static NDanmakuBullet Create(
        string spritePath, float scale, Vector2 spawnPos, float speed, float angleRad, float acc, float accAngleDeg,
        float lifetimeSeconds, IReadOnlyList<Creature> targets, Action? onHit, Color color, bool trailEnabled, Color trailColor,
        bool spawnShards, int hitAmount, float hitIntervalSeconds, bool intervalGatesFirstHit, bool zeroHitNotDie, string? onHitSfx, List<DanmakuEvent> events, bool expandOnSpawn = false)
        {
            Texture2D texture = DanmakuAssetLoader.LoadBulletSprite(spritePath);
            var bullet = new NDanmakuBullet
            {
                Texture = texture,
                Scale = Vector2.One * scale,
                GlobalPosition = spawnPos,
                Speed = speed,
                AngleRad = angleRad,
                // Add 90 degrees (Mathf.Pi / 2) so that an Up sprite points Right at 0 rads
                Rotation = angleRad + (Mathf.Pi / 2.0f),
                Acceleration = acc,
                AccelerationAngleDeg = accAngleDeg
            };
            bullet._lifetimeSeconds = lifetimeSeconds;
            bullet._targets = targets;
            bullet._onHit = onHit;
            bullet._spawnShards = spawnShards;
            bullet._hitsRemaining = Math.Max(1, hitAmount);
            bullet._hitIntervalSeconds = hitIntervalSeconds;
            bullet._lastHitTime = intervalGatesFirstHit ? 0f : float.NegativeInfinity;
            bullet._zeroHitNotDie = zeroHitNotDie;
            bullet._events = events;
            bullet._onHitSfx = onHitSfx;
            bullet.Modulate = color;
            if (trailEnabled) bullet._trail = CreateTrail(trailColor, scale);
            if (expandOnSpawn)
            {
                bullet._entering = true;
                bullet._entryTargetScale = bullet.Scale;
                bullet.Scale = Vector2.Zero;
            }
            return bullet;
        }

        public override void _Ready()
        {
            if (_trail != null)
            {
                AddChild(_trail);
            }
        }

        public override void _Process(double delta)
        {
            float dt = (float)delta * DanmakuTime.GetContinuousTimeScale();

            if (_dying)
            {
                _dyingElapsed += dt;
                float fadeT = Mathf.Clamp(_dyingElapsed / ExitFadeDurationSeconds, 0f, 1f);
                // Skips events including scaling
                Scale = _deathScale * (1f - fadeT); 
                if (fadeT >= 1f) this.QueueFreeSafely();
                return;
            }

            _elapsed += dt;

            UpdateEvents(dt);

            Speed += Acceleration * dt;
            AngleRad += Mathf.DegToRad(AccelerationAngleDeg) * dt;
            // Keeps the visuals of the bullet sprites facing up to be consistent
            Rotation = AngleRad + (Mathf.Pi / 2.0f);
            GlobalPosition += new Vector2(Mathf.Cos(AngleRad), Mathf.Sin(AngleRad)) * Speed * dt * DanmakuPiece.PixelsPerSpeedUnit;

            UpdateTrail();

            if (_entering)
            {
                _enteringElapsed += dt;
                float growT = Mathf.Clamp(_enteringElapsed / EntryGrowDurationSeconds, 0f, 1f);
                Scale = _entryTargetScale * growT; // overrides any ScaleUniform/ScaleX/etc. event this frame
                if (growT >= 1f) _entering = false;
            }

            Rect2 screenBounds = GetViewport().GetVisibleRect().Grow(64f);
            if (_elapsed > _lifetimeSeconds || !screenBounds.HasPoint(GlobalPosition))
            {
                BeginExit();
                return;
            }

            if (!HitsExhausted) CheckHits();
        }

        void UpdateEvents(float dt)
        {
            foreach (DanmakuEvent ev in _events)
            {
                if (ev.HasFinished) continue;
                if (_elapsed < ev.Start) continue;
                if (!ev.HasStarted) { ev.OnStart?.Invoke(this); ev.HasStarted = true; }

                float elapsedSinceStart = _elapsed - ev.Start;
                float t = Mathf.Clamp(elapsedSinceStart / ev.Duration, 0f, 1f);
                ev.Apply(this, elapsedSinceStart, t, dt);

                if (elapsedSinceStart >= ev.Duration)
                    ev.HasFinished = true;
            }
        }

        void CheckHits()
        {
            foreach (Creature target in _targets)
            {
                if (target.IsDead) continue;
                NCreature? node = target.GetCreatureNode();
                if (node == null || !node.Hitbox.GetGlobalRect().HasPoint(GlobalPosition)) continue;
                if (_elapsed - _lastHitTime < _hitIntervalSeconds) continue;

                _lastHitTime = _elapsed;
                _hitsRemaining--;
                _onHit?.Invoke();
                if (_onHitSfx != null)
                {
                    SfxCmd.Play(_onHitSfx);
                }

                Node? parent = GetParent();
                if (parent != null)
                {
                    if (_spawnShards) NDanmakuImpactVfx.SpawnShards(GlobalPosition, parent, Texture, Modulate);
                    else NDanmakuImpactVfx.Spawn(GlobalPosition, parent, Texture, Modulate, GlobalRotation);
                }

                if (HitsExhausted && !_zeroHitNotDie)
                {
                    BeginExit();
                }
                return;
            }
        }

        internal Vector2? GetHomingTargetPosition()
        {
            Creature? t = HomingTargetOverride ?? (_targets.Count > 0 ? _targets[0] : null);
            if (t == null || t.IsDead) return null;
            return t.GetCreatureNode()?.VfxSpawnPosition;
        }

        static Line2D CreateTrail(Color tint, float scale)
        {
            var trail = new Line2D();
            trail.TopLevel = true; // don't inherit bullet's rotation/position transform
            trail.Width = 8f * scale; 
            trail.ZIndex = -1;

            var widthCurve = new Curve();
            widthCurve.AddPoint(new Vector2(0f, 0f));   // tail: zero width
            widthCurve.AddPoint(new Vector2(1f, 1f));   // head: full width
            trail.WidthCurve = widthCurve;

            var gradient = new Gradient();
            gradient.SetColor(0, new Color(tint, 0f));   // tail: transparent
            gradient.SetColor(1, new Color(tint, 0.8f)); // head: opaque
            trail.Gradient = gradient;

            trail.Material = new CanvasItemMaterial
            {
                BlendMode = CanvasItemMaterial.BlendModeEnum.Add
            };

            return trail;
        }

        void UpdateTrail()
        {
            if (_trail == null) return;

            _trailPoints.Add((GlobalPosition, _elapsed));

            // cull points older than TrailMaxAge,
            // letting the trail shrink/retract even if the bullet has stopped moving
            _trailPoints.RemoveAll(p => _elapsed - p.Time > TrailMaxAge);

            var arr = new Vector2[_trailPoints.Count];
            for (int i = 0; i < _trailPoints.Count; i++)
                arr[i] = _trailPoints[i].Pos;

            _trail.Points = arr;
        }

        void BeginExit()
        {
            if (!_spawnShards)
            {
                _trail?.QueueFreeSafely();
                _dying = true;
                _dyingElapsed = 0f;
                _deathScale = Scale;
                return;
            }
            _trail?.QueueFreeSafely();
            this.QueueFreeSafely();
        }
    }

    public static class NDanmakuImpactVfx
    {
        const int ShardCount = 6;
        const float BaseFlashDuration = 0.18f;
        const float BaseShardDuration = 0.3f;
        const float MinShardDistance = 40f;
        const float MaxShardDistance = 70f;

        public static void Spawn(Vector2 position, Node container, Texture2D texture, Color tint, float rotation)
        {
            if (TestMode.IsOn) return;

            float duration = BaseFlashDuration / DanmakuTime.GetContinuousTimeScale();

            var flash = new Sprite2D();
            flash.Texture = texture;
            flash.Modulate = tint;
            flash.GlobalPosition = position;
            flash.Scale = Vector2.One * 0.4f;
            flash.GlobalRotation = rotation;
            container.AddChildSafely(flash);

            Tween tween = flash.CreateTween().SetParallel();
            tween.TweenProperty(flash, "scale", Vector2.One * 0.8f, duration)
                .SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Back);
            tween.TweenProperty(flash, "modulate:a", 0f, duration).SetEase(Tween.EaseType.In);
            tween.Chain().TweenCallback(Callable.From(() => flash.QueueFreeSafely()));
        }

        public static void SpawnShards(Vector2 position, Node container, Texture2D texture, Color tint)
        {
            if (TestMode.IsOn) return;

            float timeScale = DanmakuTime.GetContinuousTimeScale();
            float flashDuration = BaseFlashDuration / timeScale;
            float shardDuration = BaseShardDuration / timeScale;

            var root = new Node2D();
            root.GlobalPosition = position;
            container.AddChildSafely(root);

            var flash = new Sprite2D();
            flash.Texture = texture;
            flash.Modulate = tint;
            flash.Scale = Vector2.One * 0.4f;
            root.AddChildSafely(flash);

            Tween flashTween = flash.CreateTween().SetParallel();
            flashTween.TweenProperty(flash, "scale", Vector2.One * 0.8f, flashDuration)
                .SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Back);
            flashTween.TweenProperty(flash, "modulate:a", 0f, flashDuration).SetEase(Tween.EaseType.In);

            for (int i = 0; i < ShardCount; i++)
            {
                float baseAngle = (float)i / ShardCount * Mathf.Tau;
                float angle = baseAngle + (float)GD.RandRange(-0.3, 0.3);

                var shard = new Sprite2D();
                shard.Texture = texture;
                shard.Modulate = tint;
                shard.Scale = Vector2.One * 0.5f;
                root.AddChildSafely(shard);

                Vector2 dir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                float distance = (float)GD.RandRange(MinShardDistance, MaxShardDistance);
                Vector2 endPos = dir * distance;
                float endRotation = (float)GD.RandRange(-Mathf.Pi, Mathf.Pi);

                Tween shardTween = shard.CreateTween().SetParallel();
                shardTween.TweenProperty(shard, "position", endPos, shardDuration)
                    .SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Cubic);
                shardTween.TweenProperty(shard, "rotation", endRotation, shardDuration)
                    .SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Linear);
                shardTween.TweenProperty(shard, "modulate:a", 0f, shardDuration).SetEase(Tween.EaseType.In);
            }


            float totalDuration = Mathf.Max(flashDuration, shardDuration);
            SceneTreeTimer timer = root.GetTree().CreateTimer(totalDuration);
            timer.Connect(SceneTreeTimer.SignalName.Timeout, Callable.From(() =>
            {
                if (GodotObject.IsInstanceValid(root)) root.QueueFreeSafely();
            }));
        }
    }
}
