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
      
    public class Oasis : STS_Komachi_OnozukaCard
    {
        public override bool CanBeGeneratedInCombat => false;
        public Oasis() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
        {
            WithKeyword(CardKeyword.Exhaust);
            WithKeyword(CardKeyword.Retain, UpgradeType.Add);
            WithTip(CardKeyword.Retain);
            WithTip(KomachiKeywords.Clone);
        }

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {

            var exhaustpile = Owner.PlayerCombatState.ExhaustPile.Cards.
                Where(c=>!c.Keywords.Contains(KomachiKeywords.Clone) && !c.Keywords.Contains(KomachiKeywords.Unclonable)).ToList();
            var discardPile = Owner.PlayerCombatState.DiscardPile.Cards.
                Where(c => !c.Keywords.Contains(KomachiKeywords.Clone) && !c.Keywords.Contains(KomachiKeywords.Unclonable)).ToList();

            if (exhaustpile.Count <= 0 && discardPile.Count <= 0) return;

            CardModel? chosenCard;
            if (exhaustpile.Count == 1 && discardPile.Count <= 0)
            {
                chosenCard = exhaustpile[0];
            }
            else if (discardPile.Count == 1 && exhaustpile.Count <= 0)
            {
                chosenCard = discardPile[0];
            }
            else
            {
                var both = new List<CardModel>(exhaustpile);
                both.AddRange(discardPile);
                var choice = (await CardSelectCmd.FromSimpleGrid(
                    choiceContext,
                    both,
                    Owner,
                    new CardSelectorPrefs(SelectionScreenPrompt, 1, 1))).ToList();
                chosenCard = choice.FirstOrDefault();
            }

            if (chosenCard == null) return;
            var copy = chosenCard.CreateClone();
            copy.AddKeyword(KomachiKeywords.Clone);
            await CardPileCmd.AddGeneratedCardToCombat(copy, PileType.Hand, Owner);
            copy.GiveSingleTurnRetain();

            if (chosenCard.Type == CardType.Power || chosenCard.Keywords.Any(k => k == CardKeyword.Exhaust))
            {
                chosenCard.AddKeyword(KomachiKeywords.Clone);
            }
        }
    }
}
