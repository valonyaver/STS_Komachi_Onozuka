using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Cards.Tokens;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Commands;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Extensions;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Powers.Abilities;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Powers.Spirits;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Cards
{
      
    public class BoomBenefits : STS_Komachi_OnozukaCard
    {
        public BoomBenefits() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.AnyEnemy)
        {
            // Apply Spirits
            WithPower<VengefulSpiritPower>(nameof(Value1), 2, 2);
            // Detonate
            WithKeyword(KomachiKeywords.Detonate);
            // For every X spirits detonated, 

            // 2, get guided
            WithPower<GuidedSpiritPower>(nameof(Value2), 2);
            // 3, draw
            WithVar(nameof(Value3), 3);

        }

        
        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            await PowerCmd.Apply<VengefulSpiritPower>(choiceContext, target: cardPlay.Target, Value1, Owner.Creature, this);
            
            CardModel guidedSpiritChoice = CombatState.CreateCard<BoomBenefits>(Owner);
            ((BoomBenefits)guidedSpiritChoice).AltDescription = 1;
            CardModel drawChoice = CombatState.CreateCard<BoomBenefits>(Owner);
            ((BoomBenefits)drawChoice).AltDescription = 2;

            CardModel? chosen = await CardSelectCmd.FromChooseACardScreen(
                choiceContext,
                new List<CardModel> { guidedSpiritChoice, drawChoice },
                Owner,
                canSkip: true);


            int choice = 0;
            if (chosen is BoomBenefits chosenOption)
            {
                MainFile.Logger.LogMessage(LogLevel.Info, $"The chosen card had a choice of {chosenOption.AltDescription}. Setting this card's choice to that.", 0);
                choice = chosenOption.AltDescription;
            }


            var detonation = await DetonateCmd.Target(choiceContext, cardPlay.Target, this);


            MainFile.Logger.LogMessage(LogLevel.Info, $"The current card has a choice of {choice}", 0);
            switch (choice)
            {
                case 1:
                    var gAmount = detonation.TotalCountedAmount / 2;
                    MainFile.Logger.LogMessage(LogLevel.Info, $"Amount of Guided spirits is {gAmount}", 0);
                    await PowerCmd.Apply<GuidedSpiritPower>(choiceContext, Owner.Creature, gAmount, Owner.Creature, this);
                    break;
                case 2:
                    var dAmount = detonation.TotalCountedAmount / 3;
                    MainFile.Logger.LogMessage(LogLevel.Info, $"Amount of draw is {dAmount}", 0);
                    await CardPileCmd.Draw(choiceContext, dAmount, Owner);
                    break;
            }
        }
    }
}
