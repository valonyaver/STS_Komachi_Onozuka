using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.ValueProps;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Powers.Spirits;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Cards
{
      
    public class SpiritDefense : STS_Komachi_OnozukaCard
    {
        public SpiritDefense() : base(1, CardType.Skill, CardRarity.Common, TargetType.Self)
        {
            WithBlock(6, 1);
            WithPower<GuidedSpiritPower>(nameof(GuidedApplication), 3, 1);
        }

        public int GuidedApplication
        {
            get => DynamicVars[nameof(GuidedApplication)].IntValue;
            set
            {
                DynamicVars[nameof(GuidedApplication)].BaseValue = value;
            }
        }

        public override bool GainsBlock => true;
        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            await CreatureCmd.GainBlock(base.Owner.Creature, base.DynamicVars.Block, cardPlay, false);
            await PowerCmd.Apply<GuidedSpiritPower>(choiceContext, target: Owner.Creature, GuidedApplication, Owner.Creature, this);
        }
        //protected override void OnUpgrade()
        //{
        //    base.DynamicVars.Block.UpgradeValueBy(2m);
        //    DynamicVars[nameof(GuidedApplication)].UpgradeValueBy(2);
        //}
    }
}
