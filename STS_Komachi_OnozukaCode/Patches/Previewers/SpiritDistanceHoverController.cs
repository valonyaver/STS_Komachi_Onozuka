using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Powers.Distance;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Powers.Spirits;

namespace STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Patches;

public static class SpiritDistanceHoverController
{
    private static readonly Dictionary<NPower, NDisplacementPreviewCluster> _activeClusters = [];

    public static void OnPowerHovered(NPower powerNode)
    {
        Clear(powerNode);

        Creature? target = null;
        Creature? dealer = null;

        // Resolve Target & Dealer depending on which spirit power it is
        if (powerNode.Model is GuidedSpiritPower guided)
        {
            target = guided.DamageTarget;
            dealer = guided.Owner;
        }
        else if (powerNode.Model is VengefulSpiritPower vengeful)
        {
            target = vengeful.DamageTarget ?? vengeful.Owner;
            dealer = vengeful.Applier ?? LocalContext.GetMe(target?.CombatState)?.Creature;
        }

        if (target == null || dealer == null || target.CombatState == null)
            return;

        NCreature? targetNode = NCombatRoom.Instance?.GetCreatureNode(target);
        if (targetNode == null)
            return;

        // Get the preview from DistancePower
        var preview = DistancePower.PreviewSpiritDamage(powerNode.Model, target, dealer);
        if (preview == null)
            return;

        // Reuses your exact same NDisplacementPreviewCluster!
        var cluster = NDisplacementPreviewCluster.Create(
            preview.Value.DamageByLevel,
            preview.Value.ReachableLevels,
            preview.Value.CurrentLevel
        );

        NCombatRoom.Instance?.CombatVfxContainer.AddChildSafely(cluster);
        cluster.GlobalPosition = ComputeClusterPosition(targetNode, cluster);
        _activeClusters[powerNode] = cluster;
    }

    public static void Clear(NPower powerNode)
    {
        if (_activeClusters.TryGetValue(powerNode, out var cluster))
        {
            cluster.QueueFreeSafely();
            _activeClusters.Remove(powerNode);
        }
    }

    public static void ClearAll()
    {
        foreach (var cluster in _activeClusters.Values)
            cluster.QueueFreeSafely();
        _activeClusters.Clear();
    }

    static Vector2 ComputeClusterPosition(NCreature creatureNode, NDisplacementPreviewCluster cluster)
    {
        Vector2 hitboxTop = creatureNode.GetTopOfHitbox();
        float gapToIntent = hitboxTop.Y - creatureNode.IntentContainer.GlobalPosition.Y;
        float spacing = Mathf.Clamp(gapToIntent, 60f, 120f);
        return new Vector2(hitboxTop.X - cluster.Size.X * 0.5f, hitboxTop.Y - spacing * 2f - cluster.Size.Y);
    }
}

[HarmonyPatch(typeof(NPower), "OnHovered")]
public static class NPower_OnHovered_Patch
{
    [HarmonyPostfix]
    static void Postfix(NPower __instance) =>
        SpiritDistanceHoverController.OnPowerHovered(__instance);
}

[HarmonyPatch(typeof(NPower), "OnUnhovered")]
public static class NPower_OnUnhovered_Patch
{
    [HarmonyPostfix]
    static void Postfix(NPower __instance) =>
        SpiritDistanceHoverController.Clear(__instance);
}

[HarmonyPatch(typeof(NPower), "OnPowerRemoved")]
public static class NPower_OnPowerRemoved_Patch
{
    [HarmonyPostfix]
    static void Postfix(NPower __instance) =>
        SpiritDistanceHoverController.Clear(__instance);
}

[HarmonyPatch(typeof(NPower), "_ExitTree")]
public static class NPower_ExitTree_Patch
{
    [HarmonyPostfix]
    static void Postfix(NPower __instance) =>
        SpiritDistanceHoverController.Clear(__instance);
}