using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Cards.Tokens;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Powers.Distance;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Cards.Basics
{
      
    public class MovingDefend : STS_Komachi_OnozukaCard
    {
        public MovingDefend() : base(2, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
        {
            WithTip(typeof(DistancePower));
            WithTags(CardTag.Defend);
            WithBlock(11, 3);
            WithVar(nameof(Value1), 4, -1);
            WithTip(typeof(ManipulateDistanceToken));
        }
        public override bool GainsBlock => true;
        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            await CreatureCmd.GainBlock(base.Owner.Creature, base.DynamicVars.Block, cardPlay, false);
            foreach(var enemy in CombatState.Enemies)
            {
                var dist = DistancePower.GetLevel(enemy);
                if (dist >= Value1)
                {
                    await ManipulateDistanceToken.CreateInHand(Owner, CombatState);
                }
            }
        }
    }
}
