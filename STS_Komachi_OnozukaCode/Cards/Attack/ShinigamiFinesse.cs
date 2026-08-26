using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.ValueProps;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Commands;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Danmaku;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Extensions;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Extras;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Powers.Distance;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Cards.Basics
{

    public class ShinigamiFinesse : STS_Komachi_OnozukaCard
    {
        public ShinigamiFinesse() : base(2, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
        {
            WithDamage(6);
            WithCalculatedDamage(0, GetAttackTimes, ValueProp.Unpowered, 1);
            WithTip(typeof(DistancePower));
        }

        public override int[]? GetPossibleDisplacements()
        {
            return [0];
        }
        public static decimal GetAttackTimes(CardModel card, Creature? target)
        {
            if (target == null)
            {
                return 0m;
            }

            var distance = DistancePower.GetLevel(target);

            return distance;
        }

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");
            var hitAmount = DistancePower.GetLevel(cardPlay.Target);
            if (IsUpgraded) hitAmount++;
            await DamageCmd.Attack(DynamicVars.Damage.BaseValue).WithHitCount(hitAmount)
                .FromCard(this, cardPlay).Targeting(cardPlay.Target)
                .WithDanmaku(patterns, 0.05f)
                .Execute(choiceContext);
        }

        public override List<DanmakuPiece> patterns => [
            new DanmakuPiece {
                SpritePath = "pellet.png".BulletImagePath(),
                RootType = DanmakuRootType.Target,
                Aim = DanmakuAimType.None,   // avoid the target-aims-at-self degenerate case, per earlier fix

                // Spawn point: random angle around the target, fixed distance out
                GAngle = new GrowthValue { CustomFunc = (g, w) => (float)GD.RandRange(0.0, 360.0) },
                Radius = 400f,

                // Travel direction: roughly through-and-past the target (180° from spawn angle),
                // with jitter so it's not a dead-straight diameter every time
                RadiusA = new GrowthValue { CustomFunc = (g, w) => 180f + (float)GD.RandRange(-15.0, 15.0) },

                StartSpeed = new GrowthValue { CustomFunc = (g, w) => (float)GD.RandRange(18.0, 25.0) },
                StartAccAngle = new GrowthValue { CustomFunc = (g, w) => (float)GD.RandRange(-30.0, 30.0) }, // subtle curve
                Scale = new GrowthValue { CustomFunc = (g, w) => (float)GD.RandRange(3f, 4f) },

                ExpandOnSpawn = true,   // eases in rather than popping, matches "elegance"
                LifeSeconds = 0.3f,
                HitAmount = 1,
                HitIntervalSeconds = 0.25f,
                HitIntervalGatesFirstHit = true,
                TrailEnabled = true,
                spawnShards = false,    // plain flash impact + smooth shrink-fade exit, not a shard burst
                GatesDamage = true,
                BulletColor = StsColors.halfTransparentWhite,
            }
        ];
    }
}
