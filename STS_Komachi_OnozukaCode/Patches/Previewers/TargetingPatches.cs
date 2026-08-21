using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.Nodes.Combat;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Patches.PowerPatches;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Patches.Previewers
{

    #region Targeting Patches
    /// <summary>
    /// Patch to show damage previews when a card targets an enemy.
    /// </summary>
    [HarmonyPatch(typeof(NCard), nameof(NCard.SetPreviewTarget))]
    public static class NCard_SetPreviewTarget_Patch
    {
        [HarmonyPostfix]
        static void Postfix(NCard __instance, Creature creature)
        {
            DisplacementPreviewController.OnPreviewTargetChanged(__instance, creature);
            VengefulSpiritHoverPreview.OnPreviewTargetChanged(__instance, creature);
            ThirdAmountRaiseController.OnPreviewTargetChanged(__instance, creature);
        }
    }


    /// <summary>
    /// Patch for cards that target more than one enemy (aoe)
    /// </summary>
    [HarmonyPatch(typeof(NCardPlay), nameof(NCardPlay.ShowMultiCreatureTargetingVisuals))]
    public static class NCardPlay_ShowMultiCreatureTargetingVisuals_Patch
    {
        [HarmonyPostfix]
        static void Postfix(NCardPlay __instance)
        {
            if (__instance.CardNode != null)
                DisplacementPreviewController.OnMultiTargetPreviewRequested(__instance.CardNode);
        }
    }
    #endregion

    #region Cleanup Patches


    [HarmonyPatch(typeof(NCardPlay), nameof(NCardPlay.HideTargetingVisuals))]
    public static class NCardPlay_HideTargetingVisuals_Patch
    {
        [HarmonyPostfix]
        static void Postfix(NCardPlay __instance)
        {
            if (__instance.CardNode != null)
                DisplacementPreviewController.ClearMulti(__instance.CardNode);
        }
    }
    [HarmonyPatch(typeof(NCard), nameof(NCard.OnReturnedFromPool))]
    public static class NCard_OnReturnedFromPool_Patch
    {
        [HarmonyPostfix]
        static void Postfix(NCard __instance)
        {
            DisplacementPreviewController.ClearAllFor(__instance);
            VengefulSpiritHoverPreview.ClearPreview();
            ThirdAmountRaiseController.Clear();
        }
    }

    [HarmonyPatch(typeof(NCard), "_ExitTree")]
    public static class NCard_ExitTree_Patch
    {
        [HarmonyPostfix]
        static void Postfix(NCard __instance)
        {
            DisplacementPreviewController.ClearAllFor(__instance);
            VengefulSpiritHoverPreview.ClearPreview();
            ThirdAmountRaiseController.Clear();
        }
    }
    #endregion
}
