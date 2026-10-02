using BaseLib.Abstracts;
using BaseLib.Extensions;
using Godot;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
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
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Powers.Abilities
{
    public class InfernoTempestPower : STS_Komachi_OnozukaPower
    {
        public override PowerType Type => PowerType.Buff;
        public override PowerStackType StackType => PowerStackType.Counter;

        public override async Task BeforeHandDraw(Player player, PlayerChoiceContext choiceContext, ICombatState combatState)
        {
            if (player.Creature != Owner)
            {
                return;
            }

            var exhaustPile = Owner.Player.PlayerCombatState.ExhaustPile;

            List<CardModel> chosen = (await CardSelectCmd.FromCombatPile(
                    choiceContext,
                    exhaustPile,
                    Owner.Player,
                    new CardSelectorPrefs(SelectionScreenPrompt, 0, Math.Min(Amount, exhaustPile.Cards.Count())),
                    c => c.Rarity != CardRarity.Basic
                )).ToList();

            await ReturnCardsDistributed(choiceContext, chosen);
            await PowerCmd.Remove(this);
        }

        private async Task ReturnCardsDistributed(PlayerChoiceContext choiceContext, List<CardModel> cards)
        {
            if (cards.Count == 0) return;
            // Shuffle so both the floor-fill and the remainder are randomized.
            var rng = Owner.Player.RunState.Rng.CombatCardSelection;
            List<CardModel> shuffled = cards.OrderBy(_ => rng).ToList();

            PileType[] piles = { PileType.Hand, PileType.Draw, PileType.Discard };
            List<PileType> assignments = [];

            // Guarantee at least one card per pile, as long as there are enough cards.
            for (int i = 0; i < piles.Length && i < shuffled.Count; i++)
            {
                assignments.Add(piles[i]);
            }
            for (int i = piles.Length; i < shuffled.Count; i++)
            {
                assignments.Add(piles[rng.NextInt(piles.Length)]); 
            }

            for (int i = 0; i < shuffled.Count; i++)
            {
                await CardPileCmd.Add(shuffled[i], assignments[i]);
            }

            var exhaustBasics = Owner.Player.PlayerCombatState?.ExhaustPile.Cards.Where(c => c.Rarity == CardRarity.Basic);
            if (exhaustBasics != null && exhaustBasics.Count() > 0)
            {
                await CardPileCmd.Add(exhaustBasics, PileType.Draw, CardPilePosition.Random);
            }
        }
    }
}
