using BaseLib.Abstracts;
using BaseLib.Extensions;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.ValueProps;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Cards.Tokens;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Commands;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Extensions;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Powers.Abilities
{
    public class ExtinctFaunaPower : STS_Komachi_OnozukaPower
    {
        public override PowerType Type => PowerType.Buff;

        public override PowerStackType StackType => PowerStackType.Counter;

        protected override IEnumerable<DynamicVar> CanonicalVars => [
            new IntVar(nameof(Value1), 0), // copy effect
            new IntVar(nameof(Value2), 0), // retrieval effect
            new EnergyVar(0)
            ];

        CardModel source;
        public override async Task AfterApplied(Creature? applier, CardModel? cardSource)
        {
            source = cardSource;
        }

        public override async Task AfterPowerAmountChanged(PlayerChoiceContext choiceContext, PowerModel power, decimal amount, Creature? applier, CardModel? cardSource)
        {
            if (power != this) return;
            Value1 += amount;
            Value2 += amount;
        }

        public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            if (cardPlay.Card.Owner != Owner.Player)
            {
                return;
            }

            CardModel played = cardPlay.Card;
            if (played == source) return;

            var ExhaustPile = PileType.Exhaust.GetPile(Owner.Player);

            // Find cards with matching types (names) in the Exhaust pile
            List<CardModel> matchInExile = ExhaustPile.Cards
                .Where(c => c.GetType() == played.GetType())
                .ToList();

            // There is no match
            if ((matchInExile == null || matchInExile.Count == 0) 
                && Value1 > 0)
            {
                // copies dont get copied
                if (played.Keywords.Contains(KomachiKeywords.Clone) || played.Keywords.Contains(KomachiKeywords.Unclonable)) return;
                CardModel copy = played.CreateClone();

                copy.AddKeyword(KomachiKeywords.Clone);
                copy.EnergyCost.SetThisCombat(DynamicVars.Energy.IntValue, true);

                await CardPileCmd.AddGeneratedCardToCombat(copy, PileType.Exhaust, Owner.Player);
                await CardCmd.Exhaust(choiceContext, copy);
                Value1--;
            }
            // There is a match
            else if (matchInExile != null && matchInExile.Count > 0 
                    && Value2 > 0)
            {
                // "You can" — skip is intentional.
                CardModel? chosen = (await CardSelectCmd.FromSimpleGrid(
                    choiceContext,
                    matchInExile,
                    Owner.Player,
                    new CardSelectorPrefs(SelectionScreenPrompt, 0, 1))).FirstOrDefault();

                if (chosen != null)
                {
                    if (chosen.GetModifier<RemoveFromCombatCardModifier>() == null) CardModifier.AddModifier<RemoveFromCombatCardModifier>(chosen);
                    await CardPileCmd.Add(chosen, PileType.Hand);
                }
                Value2--;
            }
        }


        public override async Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
        {
            if (participants.Contains(Owner))
            {
                Value1 = Amount;
                Value2 = Amount;
            }
        }
    }
}
