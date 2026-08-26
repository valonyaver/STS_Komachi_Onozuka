using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Cards.Tokens;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Commands;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Extensions;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Powers.Abilities;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Powers.Distance;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Powers.Spirits;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Cards
{
    public class Sweep : STS_Komachi_OnozukaCard
    {
        public Sweep()
            : base(2, CardType.Attack, CardRarity.Common, TargetType.AllEnemies)
        {
            WithDamage(10, 4);
            WithPower<DistancePower>(nameof(Value1), -1);
            // Manipulate distances added
            WithCards(1);
            WithTip(typeof(ManipulateDistanceToken));
        }

        public override int[] GetPossibleDisplacements() => [Value1];

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            
            if (CombatState == null) return;

            IReadOnlyList<Creature> enemies = base.CombatState.HittableEnemies;
            foreach (Creature enemy in enemies)
            {
                DistanceCmd.Displace(choiceContext, enemy, Value1, Owner.Creature, this);
            }

            await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue).FromCard(this, cardPlay).TargetingAllOpponents(base.CombatState)
            .WithHitFx("vfx/vfx_giant_horizontal_slash")
            .Execute(choiceContext);
            
            for(int i = 0; i < DynamicVars.Cards.BaseValue; i++)
            {
                var mandist = CombatState.CreateCard<ManipulateDistanceToken>(Owner);
                await CardPileCmd.AddGeneratedCardToCombat(mandist, PileType.Hand, Owner);
            }
        }
    }
}
