using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
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
      
    public class CloseQuarterTechnique : STS_Komachi_OnozukaCard
    {
        public CloseQuarterTechnique() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.AnyEnemy)
        {
            WithBlock(5, 2);
            WithPower<VengefulSpiritPower>(nameof(Value1), 4, 1);
            // Strength down 
            WithVar(nameof(Value2), 3, 2);
            WithPower<GuidedSpiritPower>(nameof(Value3), 4, 1);
            WithTip(typeof(DistancePower));
        }

        public override bool GainsBlock => true;

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            await CreatureCmd.GainBlock(base.Owner.Creature, base.DynamicVars.Block, cardPlay, false);
            await PowerCmd.Apply<VengefulSpiritPower>(choiceContext, target: cardPlay.Target, Value1, Owner.Creature, this);
            if (DistancePower.GetLevel(cardPlay.Target) < 3)
            {
                await PowerCmd.Apply<CloseQuarterPower>(choiceContext, target: cardPlay.Target, -Value2, Owner.Creature, this);
                await PowerCmd.Apply<GuidedSpiritPower>(choiceContext, target: Owner.Creature, Value3, Owner.Creature, this);

            }
        }

        public class CloseQuarterPower : CustomTemporaryPowerModelWrapper<CloseQuarterTechnique, StrengthPower>
        {
            public override PowerType Type => PowerType.Debuff;

            public override bool AllowNegative => true;
        }
    }
}
