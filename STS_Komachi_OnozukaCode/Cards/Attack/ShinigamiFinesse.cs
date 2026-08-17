using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.ValueProps;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Commands;
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
            await DamageCmd.Attack(DynamicVars.Damage.BaseValue).WithHitCount(hitAmount).
                FromCard(this, cardPlay).Targeting(cardPlay.Target)
                .WithHitFx("vfx/vfx_attack_slash")
                .Execute(choiceContext);
        }
    }
}
