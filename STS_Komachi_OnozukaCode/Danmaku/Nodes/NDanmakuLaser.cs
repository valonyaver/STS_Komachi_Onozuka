using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Nodes.Combat;
using System;
using System.Collections.Generic;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Danmaku.Nodes
{

    public partial class NDanmakuLaser : Node2D
    {
        [Export] public float ExpandTime = 0.3f;
        [Export] public float ShrinkTime = 0.3f;
        [Export] public float FinalWidthPixels = 80f;
        [Export] public float LifetimeSeconds = 2.0f;

        protected ColorRect Beam;
        protected float WidthProgress { get; private set; }

        float _elapsed;
        bool _shrinkStarted;

        // Hit config, set via ConfigureHits before/just after spawn
        IReadOnlyList<Creature> _targets = [];
        Action? _onHit;
        int _hitsRemaining = 1;
        float _hitIntervalSeconds = 0.5f;
        bool _zeroHitNotDie;
        float _lastHitTime; 
        string? _onHitSfx;
        bool HitsExhausted => _hitsRemaining <= 0;

        public override void _Ready()
        {
            Beam = GetNode<ColorRect>("Beam");

            ExpandTime = Mathf.Min(LifetimeSeconds * 0.1f, ExpandTime);
            ShrinkTime = Mathf.Min(LifetimeSeconds * 0.1f, ShrinkTime);

            Beam.Modulate = _pendingTint; // apply whatever was set before _Ready ran
            Vector2 size = Beam.Size;
            size.Y = FinalWidthPixels;
            Beam.Size = size;
            Beam.Position = new Vector2(Beam.Position.X, -FinalWidthPixels / 2f);
            Beam.PivotOffset = new Vector2(0f, FinalWidthPixels / 2f);

            OnBeamReady();
            SetWidthProgressInternal(0f);

            Tween tween = CreateTween();
            tween.TweenMethod(
                Callable.From<float>(SetWidthProgressInternal),
                0.0f,
                1.0f,
                ExpandTime
            ).SetTrans(Tween.TransitionType.Cubic).SetEase(Tween.EaseType.Out);
        }

        /// <summary>Call once, right after instantiating and before (or right after) adding to the tree.</summary>
        public void ConfigureHits(IReadOnlyList<Creature> targets, 
            Action? onHit, 
            int hitAmount, float hitIntervalSeconds, bool zeroHitNotDie,
            string? onHitSfx)
        {
            _targets = targets;
            _onHit = onHit;
            _hitsRemaining = Mathf.Max(1, hitAmount);
            _hitIntervalSeconds = hitIntervalSeconds;
            _zeroHitNotDie = zeroHitNotDie;
            _onHitSfx = onHitSfx;
            _lastHitTime = -hitIntervalSeconds; // first eligible hit is at elapsed >= interval
        }

        Color _pendingTint = Colors.White;
        public void SetTint(Color color)
        {
            _pendingTint = color;
            if (Beam != null) Beam.Modulate = color; // if already ready, apply immediately
        }
        public override void _Process(double delta)
        {
            float dt = (float)delta * DanmakuTime.GetContinuousTimeScale();
            _elapsed += dt;

            if (!HitsExhausted) CheckHits();

            if (!_shrinkStarted && _elapsed >= LifetimeSeconds - ShrinkTime)
            {
                TryStartShrink();
            }
        }

        void CheckHits()
        {
            if (_elapsed - _lastHitTime < _hitIntervalSeconds) return;

            bool hitAny = false;
            foreach (Creature target in _targets)
            {
                if (target.IsDead) continue;
                NCreature? node = target.GetCreatureNode();
                if (node == null) continue;
                if (!OverlapsHitbox(node)) continue;

                hitAny = true;
                Vector2 hitPoint = GetRandomHitPointOnBeam(node);
                PlayHitFeedback(hitPoint);
            }

            if (!hitAny) return;

            _lastHitTime = _elapsed;
            _hitsRemaining--;
            _onHit?.Invoke();
            if (_onHitSfx != null)
            {
                SfxCmd.Play(_onHitSfx);
            }

            if (HitsExhausted && !_zeroHitNotDie)
            {
                TryStartShrink();
            }
            // Exhausted + ZeroHitNotDie: CheckHits just won't run again (guarded above),
            // beam stays visible/inert until its natural LifetimeSeconds shrink kicks in.
        }

        Rect2 GetHitboxLocalRect(NCreature node)
        {
            Rect2 g = node.Hitbox.GetGlobalRect();
            Transform2D toLocal = Beam.GetGlobalTransform().AffineInverse();

            // Build the local-space bounding rect by expanding to include all 4 transformed corners
            Rect2 r = new Rect2(toLocal * g.Position, Vector2.Zero);
            r = r.Expand(toLocal * (g.Position + new Vector2(g.Size.X, 0f)));
            r = r.Expand(toLocal * (g.Position + new Vector2(0f, g.Size.Y)));
            r = r.Expand(toLocal * (g.Position + g.Size));
            return r;
        }

        Rect2 GetBeamMaskRect()
        {
            float halfWidth = Beam.Size.Y / 2f;
            float currentHalfWidth = halfWidth * WidthProgress;
            return new Rect2(0f, halfWidth - currentHalfWidth, Beam.Size.X, currentHalfWidth * 2f);
        }

        bool OverlapsHitbox(NCreature node)
        {
            return GetHitboxLocalRect(node).Intersects(GetBeamMaskRect());
        }

        Vector2 GetRandomHitPointOnBeam(NCreature node)
        {
            Rect2 overlap = GetHitboxLocalRect(node).Intersection(GetBeamMaskRect());

            float randX = (float)GD.RandRange(overlap.Position.X, overlap.Position.X + overlap.Size.X);
            float randY = (float)GD.RandRange(overlap.Position.Y, overlap.Position.Y + overlap.Size.Y);

            return Beam.GetGlobalTransform() * new Vector2(randX, randY);
        }


        protected virtual void PlayHitFeedback(Vector2 worldHitPoint)
        {
            Color baseColor = Beam.Modulate;
            Color bright = new Color(
                Mathf.Min(baseColor.R * 2f, 2f),
                Mathf.Min(baseColor.G * 2f, 2f),
                Mathf.Min(baseColor.B * 2f, 2f),
                baseColor.A);

            Tween flash = CreateTween();
            flash.TweenProperty(Beam, "modulate", bright, 0.05f);
            flash.TweenProperty(Beam, "modulate", baseColor, 0.15f);

            LaserHitFlash.Spawn(worldHitPoint, GetParent(), baseColor);
        }

        void TryStartShrink()
        {
            if (_shrinkStarted) return;
            _shrinkStarted = true;

            Tween tween = CreateTween();
            tween.TweenMethod(
                Callable.From<float>(SetWidthProgressInternal),
                WidthProgress,
                0.0f,
                ShrinkTime
            ).SetTrans(Tween.TransitionType.Cubic).SetEase(Tween.EaseType.In);

            tween.TweenCallback(Callable.From(QueueFree));
        }

        void SetWidthProgressInternal(float t)
        {
            WidthProgress = t;
            OnWidthProgressChanged(t);
        }

        protected virtual void OnBeamReady() { }
        protected virtual void OnWidthProgressChanged(float t) { }
    }

    public partial class LaserHitFlash : Node2D
    {
        Color _color = Colors.White;
        float _radius = 4f;
        float _alpha = 1f;

        public static void Spawn(Vector2 globalPos, Node parent, Color color)
        {
            var flash = new LaserHitFlash { GlobalPosition = globalPos, _color = color };
            flash.Material = new CanvasItemMaterial { BlendMode = CanvasItemMaterial.BlendModeEnum.Add };
            parent.AddChild(flash);

            Tween tween = flash.CreateTween();
            tween.TweenMethod(Callable.From<float>(flash.SetRadius), 4f, 28f, 0.25f)
                .SetTrans(Tween.TransitionType.Cubic).SetEase(Tween.EaseType.Out);
            tween.Parallel().TweenMethod(Callable.From<float>(flash.SetAlpha), 1f, 0f, 0.25f);
            tween.TweenCallback(Callable.From(flash.QueueFree));
        }

        void SetRadius(float r) { _radius = r; QueueRedraw(); }
        void SetAlpha(float a) { _alpha = a; QueueRedraw(); }

        public override void _Draw()
        {
            DrawCircle(Vector2.Zero, _radius, new Color(_color, _alpha));
        }
    }
}
