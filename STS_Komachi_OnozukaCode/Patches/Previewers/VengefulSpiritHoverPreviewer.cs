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
        public static Creature? PreviewTarget { get; private set; }
        public static decimal? PreviewNewInstanceDamage { get; private set; } // only for the "no instance yet" case

        public static void OnPreviewTargetChanged(NCard card, Creature? creature)
        {
            ClearPreview();
            if (creature == null || card.Model is not STS_Komachi_OnozukaCard komachiCard) return;
            if (card.Model.TargetType != TargetType.AnyEnemy) return;

            int? stacks = komachiCard.GetVengefulSpiritStacksApplied(creature);
            if (stacks is null or <= 0) return;

            Creature applier = card.Model.Owner.Creature;
            var existing = FindOwnedInstance(creature, applier);

            // Found an existing instance
            if (existing != null)
            {
                existing.PendingStacksPreview = stacks.Value;
                foreach (var lonely in creature.Powers.OfType<LonelyBoundSpiritPower>())
                    lonely.PendingStacksPreview = stacks.Value;
                PreviewTarget = creature; // remembered so we know what to clear
                CombatManager.Instance.StateTracker.NotifyCombatStateChanged("OnPreviewTargetChanged");
                return;
            }

            // No instance for this applier — still bump any existing Lonely Bound Spirits,
            // since those don't require a live VengefulSpiritPower to exist.
            foreach (var lonely in creature.Powers.OfType<LonelyBoundSpiritPower>())
                lonely.PendingStacksPreview = stacks.Value;

            // Skip the synthetic segment if Artifact would likely eat this application outright.
            if ((creature.GetPower<ArtifactPower>()?.Amount ?? 0) > 0) { PreviewTarget = creature; return; }

            var scratch = new DamageVar("VengefulDamagePreview", 0m, ValueProp.Move);
            decimal total = KomachiHelpers.FindDamageDealt(applier, creature, stacks.Value * 2m, scratch);
            if (total <= 0) return;

            PreviewTarget = creature;
            PreviewNewInstanceDamage = total;
        }

        public static void ClearPreview()
        {
            if (PreviewTarget != null)
            {
                var existing = PreviewTarget.Powers.OfType<VengefulSpiritPower>().FirstOrDefault(p => p.PendingStacksPreview != 0);
                if (existing != null) existing.PendingStacksPreview = 0;
                foreach (var lonely in PreviewTarget.Powers.OfType<LonelyBoundSpiritPower>())
                    lonely.PendingStacksPreview = 0;
            }
            PreviewTarget = null;
            PreviewNewInstanceDamage = null;
            CombatManager.Instance.StateTracker.NotifyCombatStateChanged("ClearPreview");
        }

        static VengefulSpiritPower? FindOwnedInstance(Creature target, Creature applier)
            => target.Powers.OfType<VengefulSpiritPower>().FirstOrDefault(p => p.Applier == applier);
    }

    public class VengefulSpiritHoverForecastSource : IHealthBarForecastSource
    {
        public IEnumerable<HealthBarForecastSegment> GetHealthBarForecastSegments(HealthBarForecastContext context)
        {
            if (VengefulSpiritHoverPreview.PreviewTarget != context.Creature) yield break;
            decimal? dmg = VengefulSpiritHoverPreview.PreviewNewInstanceDamage;
            if (dmg is null or <= 0) yield break;

            yield return new HealthBarForecastSegment(
                Amount: (int)dmg.Value,
                Color: StsColors.purple, // distinct from the real power's red/purple — "if you play this," not "already pending"
                Direction: HealthBarForecastDirection.FromRight,
                Order: 1,
                OverlayMaterial: null);
        }
    }
}
