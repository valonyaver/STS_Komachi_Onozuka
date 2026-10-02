using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Cards;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Patches.Previewers;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Powers.Distance;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Patches
{
    public static class DisplacementPreviewController
    {
        static readonly Dictionary<NCard, NDisplacementPreviewCluster> _activeSingleClusters = [];
        static readonly Dictionary<NCard, List<NDisplacementPreviewCluster>> _activeMultiClusters = [];

        public static void OnPreviewTargetChanged(NCard card, Creature? creature)
        {
            ClearSingle(card);
            if (creature == null || card.Model == null) return;
            if (card.Model.TargetType != TargetType.AnyEnemy) return;

            var preview = DistancePower.PreviewDisplacementDamage(card.Model, creature);
            if (preview == null) return;

            NCreature? creatureNode = NCombatRoom.Instance?.GetCreatureNode(creature);
            if (creatureNode == null) return;

            var cluster = NDisplacementPreviewCluster.Create(preview.Value.DamageByLevel, preview.Value.ReachableLevels, preview.Value.CurrentLevel);
            NCombatRoom.Instance?.CombatVfxContainer.AddChildSafely(cluster);
            cluster.GlobalPosition = ComputeCardPosition(creatureNode, cluster);
            _activeSingleClusters[card] = cluster;
        }

        public static void OnMultiTargetPreviewRequested(NCard card)
        {
            ClearMulti(card);
            CardModel? model = card.Model;
            if (model == null || model.CombatState == null) return;
            if (model.TargetType != TargetType.AllEnemies && model.TargetType != TargetType.RandomEnemy) return;

            IReadOnlyList<Creature> enemies = model.CombatState.HittableEnemies;
            if (enemies.Count == 0) return;

            var clusters = new List<NDisplacementPreviewCluster>();
            foreach (Creature enemy in enemies)
            {
                // Recompute AS IF this enemy were the sole target, since the real UpdateVisuals
                // call for a multi-target card only ran once against an ambient target — not
                // per-enemy — so Damage.PreviewValue doesn't reflect any specific enemy here yet.
                model.DynamicVars.ClearPreview();
                model.UpdateDynamicVarPreview(CardPreviewMode.Normal, enemy, model.DynamicVars);

                var preview = DistancePower.PreviewDisplacementDamage(model, enemy);
                if (preview == null) continue;

                NCreature? creatureNode = NCombatRoom.Instance?.GetCreatureNode(enemy);
                if (creatureNode == null) continue;

                var cluster = NDisplacementPreviewCluster.Create(preview.Value.DamageByLevel, preview.Value.ReachableLevels, preview.Value.CurrentLevel);
                NCombatRoom.Instance?.CombatVfxContainer.AddChildSafely(cluster);
                cluster.GlobalPosition = ComputeCardPosition(creatureNode, cluster);
                clusters.Add(cluster);
            }

            // Restore the card's own visible preview state, since we just scribbled over it
            // per-enemy above. Matches what the real call already did before this postfix ran.
            card.UpdateVisuals(card.DisplayingPile, CardPreviewMode.MultiCreatureTargeting);

            if (clusters.Count > 0) _activeMultiClusters[card] = clusters;
        }

        public static void ClearAllFor(NCard card) { ClearSingle(card); ClearMulti(card); }

        static void ClearSingle(NCard card)
        {
            if (_activeSingleClusters.TryGetValue(card, out var existing))
            {
                existing.QueueFreeSafely();
                _activeSingleClusters.Remove(card);
            }
        }

        public static void ClearMulti(NCard card)
        {
            if (_activeMultiClusters.TryGetValue(card, out var clusters))
            {
                foreach (var c in clusters) c.QueueFreeSafely();
                _activeMultiClusters.Remove(card);
            }
        }

        static Vector2 ComputeCardPosition(NCreature creatureNode, NDisplacementPreviewCluster cluster)
        {
            Vector2 hitboxTop = creatureNode.GetTopOfHitbox();
            float gapToIntent = hitboxTop.Y - creatureNode.IntentContainer.GlobalPosition.Y;
            float spacing = Mathf.Clamp(gapToIntent, 60f, 120);
            return new Vector2(hitboxTop.X - cluster.Size.X * 0.5f, hitboxTop.Y - spacing * 2f - cluster.Size.Y);
        }
    }

    public partial class NDisplacementPreviewCluster : Control
    {
        const float ChipSize = 56f;
        const float ChipGap = 8f;

        public static NDisplacementPreviewCluster Create(IReadOnlyDictionary<int, decimal> damageByLevel, int[] reachableLevels, int currentLevel)
        {
            var reachableSet = reachableLevels.ToHashSet();
            var cluster = new NDisplacementPreviewCluster();
            float totalWidth = 5 * ChipSize + 4 * ChipGap;
            cluster.Size = new Vector2(totalWidth, ChipSize);

            for (int level = 1; level <= 5; level++)
            {
                bool reachable = reachableSet.Contains(level);
                bool isCurrent = level == currentLevel;
                decimal dmg = damageByLevel[level];

                var chip = new PanelContainer
                {
                    Size = new Vector2(ChipSize, ChipSize),
                    Position = new Vector2((level - 1) * (ChipSize + ChipGap), 0),
                    Modulate = reachable ? Colors.White : new Color(1f, 1f, 1f, 0.4f)
                };

                var vbox = new VBoxContainer { Alignment = BoxContainer.AlignmentMode.Center };
                vbox.AddChild(new Label { Text = $"Lv{level}", HorizontalAlignment = HorizontalAlignment.Center });
                vbox.AddChild(new Label
                {
                    Text = dmg.ToString("0"),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Modulate = !reachable ? Colors.Gray : (isCurrent ? Colors.White : Colors.Yellow)
                });
                chip.AddChild(vbox);
                cluster.AddChild(chip);
            }
            return cluster;
        }
    }
}
