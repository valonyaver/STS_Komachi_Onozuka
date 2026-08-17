using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Rooms;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Powers.Distance;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Patches
{
    [HarmonyPatch(typeof(NCreature), nameof(NCreature.UpdateIntent))]
    public static class NCreature_UpdateIntent_DistancePreview_Patch
    {
        [HarmonyPostfix]
        static void Postfix(NCreature __instance, IEnumerable<Creature> targets)
            => EnemyIntentDistancePreviewController.OnIntentsUpdated(__instance, targets);
    }

    [HarmonyPatch(typeof(NCreature), "_ExitTree")]
    public static class NCreature_ExitTree_DistancePreviewCleanup_Patch
    {
        [HarmonyPostfix]
        static void Postfix(NCreature __instance)
            => EnemyIntentDistancePreviewController.Remove(__instance);
    }
    public static class EnemyIntentDistancePreviewController
    {
        static readonly Dictionary<NCreature, NEnemyIntentDistanceCluster> _activeClusters = new();
        static readonly Dictionary<NCreature, List<Creature>> _lastTargets = new();
        static bool _subscribed;

        public static void OnIntentsUpdated(NCreature creatureNode, IEnumerable<Creature> targets)
        {
            EnsureSubscribed();
            _lastTargets[creatureNode] = targets.ToList();
            Refresh(creatureNode);
        }

        public static void OnDistanceChanged(Creature creature)
        {
            NCreature? node = NCombatRoom.Instance?.GetCreatureNode(creature);
            if (node != null) Refresh(node);
        }

        static void EnsureSubscribed()
        {
            if (_subscribed) return;
            _subscribed = true;
            CombatManager.Instance.StateTracker.CombatStateChanged += OnCombatStateChanged;
            CombatManager.Instance.CombatEnded += OnCombatEnded;
        }

        static void OnCombatStateChanged(CombatState _)
        {
            foreach (NCreature node in _activeClusters.Keys.ToList())
                Refresh(node);
        }

        static void OnCombatEnded(CombatRoom room)
        {
            ClearAll();
            CombatManager.Instance.StateTracker.CombatStateChanged -= OnCombatStateChanged;
            CombatManager.Instance.CombatEnded -= OnCombatEnded;
            _subscribed = false;
        }

        static void Refresh(NCreature creatureNode)
        {
            if (_activeClusters.TryGetValue(creatureNode, out var existing))
            {
                existing.QueueFreeSafely();
                _activeClusters.Remove(creatureNode);
            }
            if (!_lastTargets.TryGetValue(creatureNode, out var targets)) return;

            var preview = DistancePower.PreviewIntentDamageAcrossDistances(creatureNode.Entity, targets);
            if (preview == null) return;

            var cluster = NEnemyIntentDistanceCluster.Create(preview.Value.DamageByLevel, preview.Value.CurrentLevel);
            creatureNode.AddChildSafely(cluster);
            cluster.Position = ComputeLocalPosition(creatureNode, cluster);
            _activeClusters[creatureNode] = cluster;
        }

        static Vector2 ComputeLocalPosition(NCreature creatureNode, NEnemyIntentDistanceCluster cluster)
        {
            Vector2 intentPos = creatureNode.IntentContainer.Position;
            Vector2 intentSize = creatureNode.IntentContainer.Size;
            return new Vector2(
                intentPos.X + intentSize.X * 0.5f - cluster.Size.X * 0.5f,
                intentPos.Y - cluster.Size.Y - 12f);
        }

        public static void Remove(NCreature creatureNode)
        {
            if (_activeClusters.TryGetValue(creatureNode, out var existing))
            {
                existing.QueueFreeSafely();
                _activeClusters.Remove(creatureNode);
            }
            _lastTargets.Remove(creatureNode);
        }

        public static void ClearAll()
        {
            foreach (var c in _activeClusters.Values) c.QueueFreeSafely();
            _activeClusters.Clear();
            _lastTargets.Clear();
        }
    }
    public partial class NEnemyIntentDistanceCluster : Control
    {
        const float ChipSize = 34f; // smaller than the 56f card-hover cluster — this is secondary/ambient info
        const float ChipGap = 4f;

        public static NEnemyIntentDistanceCluster Create(IReadOnlyDictionary<int, decimal> damageByLevel, int currentLevel)
        {
            var cluster = new NEnemyIntentDistanceCluster();
            cluster.Size = new Vector2(5 * ChipSize + 4 * ChipGap, ChipSize);

            for (int level = 1; level <= 5; level++)
            {
                cluster.AddChild(new Label
                {
                    Text = damageByLevel[level].ToString("0"),
                    Position = new Vector2((level - 1) * (ChipSize + ChipGap), 0),
                    Size = new Vector2(ChipSize, ChipSize),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Modulate = level == currentLevel ? StsColors.red : StsColors.halfTransparentWhite
                });
            }
            return cluster;
        }
    }
}
