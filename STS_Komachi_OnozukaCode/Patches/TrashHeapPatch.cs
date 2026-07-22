using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Events;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Cards.Attack;
using System.Linq;

namespace STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Patches
{

    [HarmonyPatch(typeof(TrashHeap), nameof(TrashHeap.Cards), MethodType.Getter)]
    internal static class TrashHeap_Cards_AddMoveAndShoot_Patch
    {
        private static void Postfix(ref CardModel[] __result)
        {
            MainFile.LogMessage("Trash heap patch");
            __result = __result.Append(ModelDb.Card<MoveAndShoot>()).ToArray();
        }
    }
}
