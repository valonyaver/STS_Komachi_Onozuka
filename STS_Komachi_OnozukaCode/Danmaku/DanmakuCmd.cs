using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.TestSupport;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Danmaku.Nodes;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Danmaku
{
    public static class DanmakuCmd
    {
        public const float DefaultTimeoutSeconds = 2f;

        /// <summary>
        /// Fires a pattern with no hit-gating or waiting — for purely decorative bursts.
        /// </summary>
        public static Task Fire(IReadOnlyList<DanmakuPiece> pieces, Creature shooter, IReadOnlyList<Creature> targets, Control container)
        {
            if (TestMode.IsOn || targets.Count == 0 || pieces.Count == 0) return Task.CompletedTask;
            var tasks = pieces.Select(piece => RunPiece(piece, shooter, targets, container, null));
            return Task.WhenAll(tasks);
        }
        public static async Task FireAndWaitForHit(IReadOnlyList<DanmakuPiece> pieces, 
            Creature shooter, 
            IReadOnlyList<Creature> targets, 
            Control container, 
            float timeoutSeconds = DefaultTimeoutSeconds,
            Action? onHitExtra = null)
        {
            if (TestMode.IsOn || targets.Count == 0 || pieces.Count == 0)
            {
                onHitExtra?.Invoke(); // still fire the fallback so the slash isn't silently lost
                return;
            }

            bool anyGates = pieces.Any(p => p.GatesDamage);
            var tcs = new TaskCompletionSource<bool>();

            var pieceTasks = pieces.Select(piece =>
                RunPiece(piece, shooter, targets, container,
                    (piece.GatesDamage || !anyGates) ? (() => tcs.TrySetResult(true)) : null)
            ).ToList();

            Task timeoutTask = Cmd.CustomScaledWait(timeoutSeconds, timeoutSeconds);
            await Task.WhenAny(tcs.Task, timeoutTask);

            // Guaranteed fallback: regardless of whether a bullet actually connected or the
            // failsafe timer won the race, the slash always plays exactly once here.
            onHitExtra?.Invoke();
        }

        static async Task RunPiece(DanmakuPiece piece, Creature shooter, IReadOnlyList<Creature> targets, Control container, Action? onHit)
        {
            if (piece.StartTimeSeconds > 0f)
                await Cmd.CustomScaledWait(piece.StartTimeSeconds/2, piece.StartTimeSeconds);

            static async Task FireAt(DanmakuPiece piece, Creature shooter, Creature aimTarget, IReadOnlyList<Creature> allTargets, Control container, Action? onHit)
            {
                float firstGroupAimDeg = 0f;
                bool haveFirstGroupAim = false;

                for (int group = 0; group < piece.Group; group++)
                {
                    Vector2 rootPos = ResolveRoot(piece.RootType, shooter, aimTarget);

                    float aimDeg;
                    if (piece.Aim == DanmakuAimType.None)
                    {
                        aimDeg = 0f;
                    }
                    else if (piece.Aim == DanmakuAimType.FirstGroupAim)
                    {
                        if (!haveFirstGroupAim)
                        {
                            firstGroupAimDeg = AimAngleDeg(rootPos, aimTarget);
                            haveFirstGroupAim = true;
                        }
                        aimDeg = firstGroupAimDeg;
                    }
                    else // PerGroup
                    {
                        aimDeg = AimAngleDeg(rootPos, aimTarget);
                    }

                    int wayCount = (int)MathF.Round(piece.WayCount.Evaluate(group, 0));
                    if (wayCount < 1) wayCount = 1;


                    Vector2 offset = new Vector2(piece.X.Evaluate(group, 0), piece.Y.Evaluate(group, 0));
                    Vector2 basePos = rootPos + offset;

                    for (int way = 0; way < wayCount; way++)
                    {
                        float gAngle = piece.GAngle.Evaluate(group, way);
                        float range = piece.Range.Evaluate(group, way);
                        float scale = piece.Scale.Evaluate(group, way);
                        if (scale == 0f) scale = 1f;
                        float spreadDeg = 0f;
                        if (wayCount > 1)
                        {
                            float t = (float)way / (wayCount - 1);
                            spreadDeg = -range / 2f + range * t;
                        }

                        float preRadiusAngleDeg = aimDeg + gAngle + spreadDeg;
                        float preRadiusAngleRad = Mathf.DegToRad(preRadiusAngleDeg);

                        float radius = piece.Radius.Evaluate(group, way);
                        Vector2 spawnPos = basePos + radius * new Vector2(Mathf.Cos(preRadiusAngleRad), Mathf.Sin(preRadiusAngleRad));

                        float radiusA = piece.RadiusA.Evaluate(group, way);
                        float finalAngleRad = Mathf.DegToRad(preRadiusAngleDeg + radiusA);

                        float speed = piece.StartSpeed.Evaluate(group, way);
                        float acc = piece.StartAcc.Evaluate(group, way);
                        float accAngle = piece.StartAccAngle.Evaluate(group, way);
                        float lifeSeconds = piece.LifeSeconds.Evaluate(group, way);

                        if (piece.IsLaser)
                        {
                            NDanmakuLaser laser = DanmakuAssetLoader.InstantiateLaser(piece.SpritePath);
                            laser.FinalWidthPixels = piece.LaserWidthPixels * scale;
                            laser.LifetimeSeconds = lifeSeconds;
                            laser.SetTint(piece.BulletColor);
                            laser.ConfigureHits(allTargets, onHit, piece.HitAmount, piece.HitIntervalSeconds, piece.ZeroHitNotDie, piece.OnHitSfx);

                            container.AddChildSafely(laser);
                            laser.GlobalPosition = spawnPos;
                            laser.GlobalRotationDegrees = preRadiusAngleDeg + radiusA;
                        }
                        else
                        {
                            Color trailColor = piece.TrailColor ?? piece.BulletColor;
                            List<DanmakuEvent> resolvedEvents = piece.Events.Select(e => e.Resolve(group, way)).ToList();

                            NDanmakuBullet bullet = NDanmakuBullet.Create(
                                piece.SpritePath, scale, spawnPos, speed, finalAngleRad, acc, accAngle, lifeSeconds,
                                allTargets, onHit, piece.BulletColor, piece.TrailEnabled, trailColor,
                                piece.spawnShards, piece.HitAmount, piece.HitIntervalSeconds, piece.ZeroHitNotDie, piece.OnHitSfx, resolvedEvents);
                            container.AddChildSafely(bullet);
                        }
                    }

                    bool isLastGroup = group == piece.Group - 1;
                    if (!isLastGroup)
                    {
                        float interval = piece.GIntervalSeconds.Evaluate(group, 0);
                        if (interval > 0f)
                            await Cmd.CustomScaledWait(interval / 2f, interval);
                    }
                }
            }

            if (piece.RepeatPerTarget)
            {
                foreach (Creature target in targets)
                    await FireAt(piece, shooter, target, targets, container, onHit);
                // If want simultaneous
                // await Task.WhenAll(targets.Select(FireAt));
            }
            else
            {
                await FireAt(piece, shooter, targets[0], targets, container, onHit);
            }
        }

        static Vector2 ResolveRoot(DanmakuRootType rootType, Creature shooter, Creature target) => rootType switch
        {
            DanmakuRootType.Shooter => shooter.GetCreatureNode()?.VfxSpawnPosition + new Vector2(20, 0) ?? Vector2.Zero,
            DanmakuRootType.Target => target.GetCreatureNode()?.VfxSpawnPosition ?? Vector2.Zero,
            DanmakuRootType.World => Vector2.Zero,
            _ => Vector2.Zero,
        };

        static float AimAngleDeg(Vector2 from, Creature target)
        {
            Vector2 targetPos = target.GetCreatureNode()?.VfxSpawnPosition ?? from;
            return Mathf.RadToDeg((targetPos - from).Angle());
        }
    }
}
