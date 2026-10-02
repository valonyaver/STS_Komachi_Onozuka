using BaseLib.Hooks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.ValueProps;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Cards;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Extras;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Powers.Abilities;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Powers.Spirits;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Patches.Previewers
{
    public static class VengefulSpiritHoverPreview
    {
        // Every creature we've touched this preview session (for cleanup).
        static readonly HashSet<Creature> _touched = [];
        // Only for targets with no existing instance: creature -> forecast damage.
        static readonly Dictionary<Creature, decimal> _newInstanceDamage = [];

        public static bool IsPreviewing(Creature creature) => _touched.Contains(creature);

        public static bool TryGetNewInstanceDamage(Creature creature, out decimal damage)
            => _newInstanceDamage.TryGetValue(creature, out damage);

        // ---- Single target (AnyEnemy) ----
        public static void OnPreviewTargetChanged(NCard card, Creature? creature)
        {
            bool hadState = ResetState();

            bool touched = false;
            if (creature != null
                && card.Model is STS_Komachi_OnozukaCard komachiCard
                && card.Model.TargetType == TargetType.AnyEnemy)
            {
                touched = PreviewOn(komachiCard, creature);
            }

            if (hadState || touched) Notify("OnPreviewTargetChanged");
        }

        // ---- Multi target (AllEnemies / RandomEnemy) ----
        public static void OnMultiTargetPreviewRequested(NCard card)
        {
            bool hadState = ResetState();

            bool touched = false;
            if (card.Model is STS_Komachi_OnozukaCard k
                && (k.TargetType == TargetType.AllEnemies || k.TargetType == TargetType.RandomEnemy)
                && k.CombatState != null)
            {
                foreach (Creature enemy in k.CombatState.HittableEnemies)
                    touched |= PreviewOn(k, enemy);
            }

            if (hadState || touched) Notify("OnMultiTargetPreviewRequested");
        }

        static bool PreviewOn(STS_Komachi_OnozukaCard card, Creature creature)
        {
            int? stacks = card.GetVengefulSpiritStacksApplied(creature);
            if (stacks is null or <= 0) return false;
            if ((creature.GetPower<ArtifactPower>()?.Amount ?? 0) > 0) return false;

            Creature applier = card.Owner.Creature;
            _touched.Add(creature);

            foreach (var lonely in creature.Powers.OfType<LonelyBoundSpiritPower>())
                lonely.PendingStacksPreview = stacks.Value;

            var existing = FindOwnedInstance(creature, applier);
            if (existing != null)
            {
                existing.PendingStacksPreview = stacks.Value;
                return true;
            }

            decimal total = KomachiHelpers.FindDamageDealt(applier, creature, stacks.Value);
            if (total > 0) _newInstanceDamage[creature] = total;
            return true;
        }

        // Public API (used by your ExitTree / pool patches)
        public static void ClearPreview()
        {
            if (ResetState()) Notify("ClearPreview");
        }

        // Returns true if there was anything to clear.
        static bool ResetState()
        {
            bool hadState = _touched.Count > 0;
            foreach (var creature in _touched)
            {
                foreach (var vs in creature.Powers.OfType<VengefulSpiritPower>())
                    if (vs.PendingStacksPreview != 0) vs.PendingStacksPreview = 0;
                foreach (var lonely in creature.Powers.OfType<LonelyBoundSpiritPower>())
                    lonely.PendingStacksPreview = 0;
            }
            _touched.Clear();
            _newInstanceDamage.Clear();
            return hadState;
        }

        static void Notify(string reason)
            => CombatManager.Instance.StateTracker.NotifyCombatStateChanged(reason);

        static VengefulSpiritPower? FindOwnedInstance(Creature target, Creature applier)
            => target.Powers.OfType<VengefulSpiritPower>().FirstOrDefault(p => p.Applier == applier);
    }

    public class VengefulSpiritHoverForecastSource : IHealthBarForecastSource
    {
        public IEnumerable<HealthBarForecastSegment> GetHealthBarForecastSegments(HealthBarForecastContext context)
        {
            if (!VengefulSpiritHoverPreview.TryGetNewInstanceDamage(context.Creature, out decimal dmg) || dmg <= 0)
                yield break;

            yield return new HealthBarForecastSegment(
                Amount: (int)dmg,
                Color: StsColors.purple,
                Direction: HealthBarForecastDirection.FromRight,
                Order: 1,
                OverlayMaterial: null);
        }
    }
}
