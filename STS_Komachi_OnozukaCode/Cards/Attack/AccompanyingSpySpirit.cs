using BaseLib.Extensions;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using STS_Komachi_Onozuka;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Danmaku;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Extensions;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Extras;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Powers.Abilities;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Powers.Distance;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Powers.Spirits;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Cards.Attack
{
      
    public class AccompanyingSpySpirit : STS_Komachi_OnozukaCard
    {
        public AccompanyingSpySpirit()
        : base(2, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
        {
            WithDamage(13, 3);
            WithKeyword(KomachiKeywords.Displace);
            WithKeyword(KomachiKeywords.Barrier);
            WithKeyword(CardKeyword.Exhaust);
            WithTip(typeof(DistancePower));
            // Summon from displacement
            WithVar(nameof(Value1), 1, 1);
        }
        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");
            await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue).FromCard(this, cardPlay).Targeting(cardPlay.Target)
                .WithDanmaku(patterns, 3)
                .WithHitFx("vfx/vfx_attack_slash")
                .Execute(choiceContext);
            await PowerCmd.Apply<AccompanyingSpySpiritPower>(
                choiceContext, cardPlay.Target, 
                Value1, Owner.Creature, this);
        }

        public override List<DanmakuPiece> patterns => [
        new DanmakuPiece {
                SpritePath = "circle_halo.png".BulletImagePath(),
                RootType = DanmakuRootType.Shooter,
                Aim = DanmakuAimType.PerGroup,
                BulletColor = StsColors.blueGlow,
                StartSpeed = 12,
                GAngle = 0,
                HitAmount = 1,      
                HitIntervalSeconds = 2.2f,
                Y = 0,
                HitIntervalGatesFirstHit = true,
                TrailEnabled = true,
                GatesDamage = true,  
                LifeSeconds = 3f,
                spawnShards = true,
                Events = [
                        FlyThenSpiralIn(
                            captureRadius: 350f,
                            angularSpeedDegPerSec: 420f,
                            closeSpeed: 120f,
                            easeInSeconds: 0.2f,
                            start: 0f,
                            duration: 2.5f),
                        DanmakuEvents.ScaleUniform(0.4f, start: 0f, duration: 2f, mode: DanmakuEventMode.Add), // grows +0.4 scale over the flight
                    ],
            },
            new DanmakuPiece {
                SpritePath = "pellet.png".BulletImagePath(), // swap for whatever small glow asset you have
                RootType = DanmakuRootType.Shooter,
                Aim = DanmakuAimType.PerGroup,
                BulletColor = StsColors.blue,
                Y = 0,
                X = 20,
                GAngle = new GrowthValue { CustomFunc = (g, w) => GD.RandRange(-90, 90) + 180 },
                StartSpeed = 12,
                Group = 6,
                GIntervalSeconds = 0.25f,
                Events = [
                    DanmakuEvents.Homing(360, 0, 1)
                    ],
                TrailEnabled = true,
                GatesDamage = false,
                spawnShards = true,
            },
        ];

        /// <summary>
        /// Flies straight (normal Speed/Angle) until within captureRadius of the current
        /// homing target, then takes over position: orbits the target while shrinking
        /// the radius, producing a spiral-in. Fully distance-driven — no dependency on
        /// how long the straight-flight phase actually took, so it works the same at
        /// any Distance level. The real hit is left to the bullet's own hitbox
        /// collision once the radius decays near zero.
        /// </summary>
        public static DanmakuEventTemplate FlyThenSpiralIn(
            GrowthValue captureRadius, GrowthValue angularSpeedDegPerSec, GrowthValue closeSpeed,
            GrowthValue easeInSeconds, GrowthValue start, GrowthValue duration, bool clockwise = true)
        {
            return new DanmakuEventTemplate
            {
                Resolve = (group, way) =>
                {
                    float capture = captureRadius.Evaluate(group, way);
                    float targetAngularSpeed = Mathf.DegToRad(angularSpeedDegPerSec.Evaluate(group, way)) * (clockwise ? 1f : -1f);
                    float targetCloseRate = closeSpeed.Evaluate(group, way);
                    float easeIn = Mathf.Max(0.0001f, easeInSeconds.Evaluate(group, way));

                    bool spiraling = false;
                    float captureElapsed = 0f;
                    float entryAngularSpeed = 0f;
                    float entryCloseRate = 0f;

                    return new DanmakuEvent
                    {
                        Start = start.Evaluate(group, way),
                        Duration = Mathf.Max(0.05f, duration.Evaluate(group, way)),
                        Apply = (b, elapsed, t, dt) =>
                        {
                            Vector2? targetPos = b.GetHomingTargetPosition();
                            if (targetPos == null) return;

                            Vector2 toBullet = b.GlobalPosition - targetPos.Value;
                            float radius = toBullet.Length();

                            if (!spiraling)
                            {
                                if (radius > capture) return; // still the straight-flight leg
                                spiraling = true;

                                // Decompose current velocity into radial/tangential so the orbit
                                // starts at whatever speed the bullet actually arrived with.
                                Vector2 radialDir = toBullet.Normalized();
                                Vector2 tangentDir = new Vector2(-radialDir.Y, radialDir.X); // 90° CCW
                                Vector2 velocityPxPerSec =
                                    new Vector2(Mathf.Cos(b.AngleRad), Mathf.Sin(b.AngleRad))
                                    * b.Speed * DanmakuPiece.PixelsPerSpeedUnit;

                                float radialSpeedPx = velocityPxPerSec.Dot(radialDir);   // + = moving away
                                float tangentSpeedPx = velocityPxPerSec.Dot(tangentDir);

                                entryCloseRate = -radialSpeedPx; // flip: + = closing
                                entryAngularSpeed = radius > 1f ? tangentSpeedPx / radius : targetAngularSpeed;
                            }

                            captureElapsed += dt;
                            float easeT = Mathf.Clamp(captureElapsed / easeIn, 0f, 1f);
                            float angularSpeed = Mathf.Lerp(entryAngularSpeed, targetAngularSpeed, easeT);
                            float closeRate = Mathf.Lerp(entryCloseRate, targetCloseRate, easeT);

                            if (radius < 1f) return; // effectively on the hitbox; collision finishes it

                            float newRadius = Mathf.Max(0f, radius - closeRate * dt);
                            Vector2 newOffset = toBullet.Rotated(angularSpeed * dt).Normalized() * newRadius;

                            b.AngleRad = (newOffset - toBullet).Angle();
                            b.GlobalPosition = targetPos.Value + newOffset;
                            b.Speed = 0f; // position is fully event-driven now; stop base integration
                        },
                    };
                }
            };
        }
    }
}
