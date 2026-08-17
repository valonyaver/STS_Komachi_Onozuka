using Godot;
using HarmonyLib;
using MegaCrit.Sts2.addons.mega_text;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Combat;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Patches.Previewers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Patches.PowerPatches
{

    public interface IHasThirdAmount
    {
        /// <summary>
        /// What the third amount should show.
        /// </summary>
        /// <returns></returns>
        decimal? GetThirdAmount();


        /// <summary>
        /// Override to have this power's third-amount label raise above the healthbar/
        /// nameplate area while `hoveredCard` is being targeted at this power's Owner.
        /// Triggered by card-hover targeting (same as the displacement/vengeful preview system). 
        /// Default false — only opt in
        /// for powers where hovering a specific card genuinely changes what GetThirdAmount
        /// would report.
        /// </summary>
        bool ShouldRaiseThirdAmount(CardModel? hoveredCard) => false;
    }


    internal static class NPower_ThirdAmount_Patch
    {
        static readonly Dictionary<NPower, Action<CombatState>> _stateHandlers = new();

        [HarmonyPatch(typeof(NPower), "RefreshAmount")]
        [HarmonyPostfix]
        static void Postfix(NPower __instance)
        {
            if (!__instance.IsNodeReady()) return;

            if (__instance.Model is not IHasThirdAmount ambient)
            {
                if (__instance.Model != null)
                    ThirdAmountFloatingLabelController.Clear(__instance.Model);
                return;
            }

            decimal? damage = ambient.GetThirdAmount();
            if (damage == null)
            {
                ThirdAmountFloatingLabelController.Clear(__instance.Model);
                return;
            }

            var amountLabel = __instance.GetNode<MegaLabel>("%AmountLabel");
            bool shouldRaise = ThirdAmountRaiseController.ShouldRaise(__instance.Model);

            ThirdAmountFloatingLabelController.Refresh(__instance, damage.Value, shouldRaise, amountLabel);
        }

        [HarmonyPatch(typeof(NPower), "SubscribeToModelEvents")]
        [HarmonyPostfix]
        static void Subscribe(NPower __instance)
        {
            if (__instance.Model is not IHasThirdAmount) return;
            if (_stateHandlers.ContainsKey(__instance)) return;

            Action<CombatState> handler = _ => __instance.RefreshAmount();
            CombatManager.Instance.StateTracker.CombatStateChanged += handler;
            _stateHandlers[__instance] = handler;
        }

        [HarmonyPatch(typeof(NPower), "UnsubscribeFromModelEvents")]
        [HarmonyPostfix]
        static void Unsubscribe(NPower __instance)
        {
            if (_stateHandlers.TryGetValue(__instance, out var handler))
            {
                CombatManager.Instance.StateTracker.CombatStateChanged -= handler;
                _stateHandlers.Remove(__instance);
            }

            if (__instance.Model != null)
            {
                ThirdAmountFloatingLabelController.Clear(__instance.Model);
            }
        }
    }
}
