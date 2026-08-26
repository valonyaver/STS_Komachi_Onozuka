using Godot;
using MegaCrit.Sts2.Core.Audio.Debug;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Creatures;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Danmaku;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Extras
{
    public static class AttackCommandExtensions
    {
        /// <summary>
        /// Plays the danmaku and delays the sfx and vfx of the attack to the end of the danmaku.
        /// Try to put it at the end of the sequence.
        /// </summary>
        /// <returns></returns>
        public static AttackCommand WithDanmaku(this AttackCommand builder, List<DanmakuPiece> pattern, float timeoutSeconds = DanmakuCmd.DefaultTimeoutSeconds)
        {
            string? deferredHitVfx = builder.HitVfx;
            string? deferredHitSfx = builder.HitSfx;
            string? deferredTmpHitSfx = builder.TmpHitSfx;

            if (builder.HitVfx != null)
            {
                builder.HitVfx = null;
            }
            if (builder.HitSfx != null)
            {
                builder.HitSfx = null;
            }
            if (builder.TmpHitSfx != null)
            {
                builder.TmpHitSfx = null;
            }
            builder._beforeDamage += async () =>
            {
                Creature? shooter = builder.Attacker;
                IReadOnlyList<Creature> targets = builder.GetPossibleTargets();
                if (shooter == null || targets.Count == 0) return;

                // Snapshot + clear whatever WithHitFx set, so AttackCommand.Execute's own
                // HitVfx playback (which happens earlier in its pipeline, before BeforeDamage
                // ever runs) doesn't fire it early. We replay it ourselves once the danmaku
                // actually resolves.

                if (builder.HitVfx != null)
                {
                    deferredHitVfx = builder.HitVfx;
                    builder.HitVfx = null;
                }

                if (builder.HitSfx != null)
                {
                    deferredHitSfx = builder.HitSfx;
                    builder.HitSfx = null;
                }


                if (builder.TmpHitSfx != null)
                {
                    deferredTmpHitSfx = builder.TmpHitSfx;
                    builder.TmpHitSfx = null;
                }

                Control? container = shooter.GetVfxContainer();
                if (container == null) return;

                await DanmakuCmd.FireAndWaitForHit(
                    pattern,
                    shooter,
                    targets,
                    container,
                    timeoutSeconds: timeoutSeconds,
                    onHitExtra: () =>
                    {
                        if (deferredHitSfx != null)
                        {
                            SfxCmd.Play(deferredHitSfx);
                        }
                        else if (deferredTmpHitSfx != null)
                        {
                            NDebugAudioManager.Instance?.Play(deferredTmpHitSfx);
                        }

                        if (deferredHitVfx == null) return;
                        // Same play pattern AttackCommand itself would've used — single vs.
                        // multi target mirrors the _spawnVfxOnEachCreature branch in Execute.
                        if (targets.Count == 1)
                        {
                            VfxCmd.PlayOnCreatureCenter(targets[0], deferredHitVfx);
                        }
                        else
                        {
                            VfxCmd.PlayOnCreatureCenters(targets, deferredHitVfx);
                        }
                    });
            };
            return builder;
        }
    }
}
