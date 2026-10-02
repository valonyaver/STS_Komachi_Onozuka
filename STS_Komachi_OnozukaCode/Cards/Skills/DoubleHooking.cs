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

    public class DoubleHooking : STS_Komachi_OnozukaCard
    {
        public override bool CanBeGeneratedInCombat => false;

        public DoubleHooking() : base(2, CardType.Skill, CardRarity.Rare, TargetType.Self)
        {
            WithCostUpgradeBy(-1);
            WithKeyword(CardKeyword.Exhaust);
        }

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            CardModel? discard = (await CardSelectCmd.FromHandForDiscard(
                choiceContext,
                base.Owner,
                new CardSelectorPrefs(CardSelectorPrefs.DiscardSelectionPrompt, 1),
                null,
                this
            )).FirstOrDefault();

            if (discard != null)
            {
                await CardCmd.Discard(choiceContext, discard);
            }

            var combatState = Owner?.PlayerCombatState;
            if (combatState == null) return;

            var exhaustpile = combatState.ExhaustPile.Cards
                .Where(c => c is not DoubleHooking)
                .ToList();

            if (exhaustpile.Count <= 0) return;

            CardModel? toHand = null;
            CardModel? toDiscard = null;

            if (exhaustpile.Count == 1)
            {
                toHand = exhaustpile[0];
            }
            else
            {
                var choice = (await CardSelectCmd.FromCombatPile(
                    choiceContext,
                    combatState.ExhaustPile,
                    Owner,
                    new CardSelectorPrefs(SelectionScreenPrompt, 0, 2),
                    c => c is not DoubleHooking
                )).ToList();

                // Only if the player actually selected cards
                if (choice.Count > 0)
                {
                    toHand = choice[0];
                }
                if (choice.Count > 1)
                {
                    toDiscard = choice[1];
                }
            }

            // Move cards and set cost until played
            if (toHand != null)
            {
                await CardPileCmd.Add(toHand, PileType.Hand, CardPilePosition.Bottom, null, false);
                toHand.EnergyCost.SetUntilPlayed(0, false);
            }
            if (toDiscard != null)
            {
                await CardPileCmd.Add(toDiscard, PileType.Discard, CardPilePosition.Bottom, null, false);
                toDiscard.EnergyCost.SetUntilPlayed(0, false);
            }
        }
    }
}
