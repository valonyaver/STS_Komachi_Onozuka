using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Potions;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using MegaCrit.Sts2.GameInfo.Objects;
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
      
    public class AfterlifePassage : STS_Komachi_OnozukaCard
    {
        public AfterlifePassage() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
        {
            WithKeyword(CardKeyword.Exhaust);
            WithPower<GuidedSpiritPower>(nameof(Value1), 4);
            WithPower<DivineSpiritPower>(nameof(Value2), 4);
            WithKeyword(KomachiKeywords.Barrier);
            // Cards to exhaust
            WithVar(nameof(Value3), 2);
        }

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            List<CardModel> list = [.. Owner.PlayerCombatState.Hand.Cards.Where(c => c != this)];

            List<CardModel> choice;
            if (IsUpgraded)
            {
                list.AddRange(Owner.PlayerCombatState.DiscardPile.Cards);
                choice = [.. (await CardSelectCmd.FromSimpleGrid(
                    choiceContext,
                    list,
                    Owner,
                    new CardSelectorPrefs(SelectionScreenPrompt, 1, Value3)))];
            }
            else
            {
                choice = [.. await CardSelectCmd.FromHand(choiceContext, Owner, new CardSelectorPrefs(SelectionScreenPrompt, 1, Value3), null, this)];
            }

            if (choice.Count > 0)
            {
                foreach (var card in choice)
                {
                    await CardCmd.Exhaust(choiceContext, card);
                    if (card.Type == CardType.Status || card.Type == CardType.Curse || card.Rarity == CardRarity.Basic)
                    {
                        await PowerCmd.Apply<GuidedSpiritPower>(choiceContext, target: Owner.Creature, Value1, Owner.Creature, this);
                    }
                    else
                    {
                        await PowerCmd.Apply<DivineSpiritPower>(choiceContext, target: Owner.Creature, Value2, Owner.Creature, this);
                    }
                }
            }
        }
    }
}
