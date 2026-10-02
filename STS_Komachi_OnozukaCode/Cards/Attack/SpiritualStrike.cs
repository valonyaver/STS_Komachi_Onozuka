using BaseLib.Extensions;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Danmaku;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Extensions;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Extras;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Powers.Spirits;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Cards.Attack
{
      
    public class SpiritualStrike : STS_Komachi_OnozukaCard
    {
        public SpiritualStrike()
        : base(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
        {
            WithDamage(5, 2);
            // Just to get its tooltip lol.
            WithPower<VengefulSpiritPower>(0);
            WithTags(CardTag.Strike);
        }

        public override int? GetVengefulSpiritStacksApplied(Creature target)
        {
            return (int) Hook.ModifyDamage(RunState, CombatState, target, Owner.Creature, 
                DynamicVars.Damage.BaseValue, DynamicVars.Damage.Props, this, null, ModifyDamageHookType.All, CardPreviewMode.None, out _);
        }
        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");
            danmakuTarget = cardPlay.Target;
            AttackCommand attackCommand = await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue).FromCard(this, cardPlay).Targeting(cardPlay.Target)
                .WithDanmaku(patterns)
                .WithHitFx("vfx/vfx_attack_slash")
                .Execute(choiceContext);

            var spiritstoApply = attackCommand.Results.SelectMany((List<DamageResult> r) => r).Sum((DamageResult r) => r.TotalDamage) * 2;

            await PowerCmd.Apply<VengefulSpiritPower>(choiceContext, cardPlay.Target, spiritstoApply, base.Owner.Creature, this);

            var spirits = cardPlay.Target.GetPower<VengefulSpiritPower>();
            if (spirits != null)
            {
                spirits.Duration++;
            }
        }

        Creature danmakuTarget;
        public override List<DanmakuPiece> patterns
        {
            get
            {
                if (danmakuTarget == null) return [];
                int count = Mathf.Clamp(GetVengefulSpiritStacksApplied(danmakuTarget).Value, 1, 30);

                // 1 frame at 60 FPS is ~0.01667 seconds
                const float minStagger = 1f / 60f;
                const float maxStagger = 0.12f; // Stagger duration when count is 1
                const float decayRate = 0.1f;  // Higher number = speeds up faster as count grows

                // Calculate exponential stagger
                float stagger = minStagger + (maxStagger - minStagger) * Mathf.Exp(-decayRate * (count - 1));

                const float postSpawnOrbit = 0.4f;
                float totalSpawnSpan = (count - 1) * stagger;

                return [
                    new DanmakuPiece {
                        SpritePath = "pellet.png".BulletImagePath(),
                        BulletColor = StsColors.purple,
                        RootType = DanmakuRootType.Shooter,
                        Aim = DanmakuAimType.None,   // position is fully driven by the event
                        Group = count,
                        GIntervalSeconds = stagger,
                        ExpandOnSpawn = true,
                        LifeSeconds = totalSpawnSpan + postSpawnOrbit + 2f, // orbit window + generous flight time
                        HitAmount = 1,
                        HitIntervalSeconds = 0.15f,
                        HitIntervalGatesFirstHit = true,
                        TrailEnabled = true,
                        spawnShards = true,
                        GatesDamage = true,
                        Events = [
                            OrbitThenLaunch(
                                radius: 180f,
                                startAngleDeg: new GrowthValue(0f, perGroup: 720f / count ), // spreads bullets evenly as they appear
                                angularSpeedDegPerSec: 260f,
                                launchSpeed: 14f,
                                start: 0f,
                                duration: new GrowthValue(totalSpawnSpan + postSpawnOrbit)
                            )
                        ],
                    }
                ];
            }
        }

        public static DanmakuEventTemplate OrbitThenLaunch(
            GrowthValue radius, GrowthValue startAngleDeg, GrowthValue angularSpeedDegPerSec,
            GrowthValue launchSpeed, GrowthValue start, GrowthValue duration)
        {
            return new DanmakuEventTemplate
            {
                Resolve = (group, way) =>
                {
                    float r = radius.Evaluate(group, way);
                    float startAngle = startAngleDeg.Evaluate(group, way);
                    float angularSpeed = angularSpeedDegPerSec.Evaluate(group, way);
                    float speed = launchSpeed.Evaluate(group, way);
                    Vector2 anchor = Vector2.Zero;

                    return new DanmakuEvent
                    {
                        Start = start.Evaluate(group, way),
                        Duration = Mathf.Max(0.05f, duration.Evaluate(group, way)),
                        OnStart = b => anchor = b.GlobalPosition, // spawn point IS the shooter's position
                        Apply = (b, elapsed, t, dt) =>
                        {
                            if (t >= 1f)
                            {
                                Vector2? targetPos = b.GetHomingTargetPosition();
                                if (targetPos != null)
                                    b.AngleRad = (targetPos.Value - b.GlobalPosition).Angle();
                                b.Speed = speed; 
                                return;
                            }

                            float angleRad = Mathf.DegToRad(startAngle + angularSpeed * elapsed);
                            b.GlobalPosition = anchor + r * new Vector2(Mathf.Cos(angleRad), Mathf.Sin(angleRad));
                            b.Speed = 0f; // fully position-driven while orbiting
                        },
                    };
                }
            };
        }
    }
}
