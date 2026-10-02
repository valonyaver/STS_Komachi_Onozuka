using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Screens.CardSelection;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Character;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Patches
{

    /// <summary>
    /// Lets you right click cards in selections (why doesn't the basegame do this bruh)
    /// </summary>
    [HarmonyPatch(typeof(NCombatPileCardSelectScreen), "UpdatePileContents")]
    internal static class SelectionScreenInspectCardPatch
    {

        [HarmonyPostfix]
        private static void SyncInspectCards(NCombatPileCardSelectScreen __instance)
        {
            var pile = __instance._pile;
            if (pile == null || pile.Cards.Count == 0 || pile.Cards[0].Owner.Character is not Komachi_Character)
                return;

            IEnumerable<CardModel> cards = pile.Cards;
            if (__instance._filter != null)
            {
                cards = cards.Where(__instance._filter);
            }

            // If it's the Draw Pile, sort it to prevent leaking the deck's true draw order.
            if (pile.Type == PileType.Draw)
            {
                cards = cards.OrderBy(c => c.Rarity).ThenBy(c => c.Id);
            }

            __instance._cards = cards.ToList();
        }
    }
}
