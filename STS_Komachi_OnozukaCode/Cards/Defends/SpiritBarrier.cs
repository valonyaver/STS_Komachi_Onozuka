using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.ValueProps;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Commands;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Extensions;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Minions;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Powers.Spirits;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Cards
{
      
    public class SpiritBarrier : STS_Komachi_OnozukaCard
    {
        public SpiritBarrier() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
        {
            WithBlock(8, 2);
            WithHeal(3, 2);
            WithKeyword(KomachiKeywords.Release);
            WithKeyword(KomachiKeywords.Barrier);

            // Player must have at least 1 spirit
            WithVar(nameof(ReleaseCost), 1);
        }
        public override bool GainsBlock => true;
        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {

            CardModel releaseChoice = await ReleaseCmd.ChooseRelease(choiceContext, this, ReleaseCost);

            await CreatureCmd.GainBlock(base.Owner.Creature, base.DynamicVars.Block, cardPlay, false);
            var divineSpirit = Owner.PlayerCombatState.GetPet<DivineSpiritMinion>();
            if (divineSpirit != null)
            {
                await CreatureCmd.Heal(divineSpirit, DynamicVars.Heal.BaseValue); 
            }

            int guided = Owner.Creature.GetPower<GuidedSpiritPower>()?.Amount ?? 0;
            int divine = Owner.Creature.GetPower<DivineSpiritPower>()?.Amount ?? 0;
            int total = guided + divine;

            if (ReleaseCmd.ChoseRelease(releaseChoice))
            {
                await ReleaseCmd.Release(choiceContext, Owner.Creature, total, this);

                await DivineSpiritCmd.Summon(choiceContext, Owner, total, this);
            }
        }
    }
}
