using Godot;
using HarmonyLib;
using MegaCrit.Sts2.addons.mega_text;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Combat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Patches.PowerPatches
{
    /// <summary>
    /// Must be used with IHasSecondAmount
    /// Gives it a custom colour and a custom tweening condition.
    /// </summary>
    public interface IHasColoredSecondAmount
    {
        Color SecondAmountColor { get; }
        bool ShouldEmphasizeSecondAmount { get; }
    }

    internal static class NPower_SecondAmountStyling_Patch
    {
        static readonly Dictionary<NPower, Tween> _activeTweens = new();

        [HarmonyPatch(typeof(NPower), "RefreshAmount")]
        [HarmonyPostfix]
        static void Postfix(NPower __instance)
        {
            if (!__instance.IsNodeReady()) return;
            if (__instance.Model is not IHasColoredSecondAmount emphasized) return;
            if (!__instance.HasNode("Amount2Label")) return;

            var label2 = __instance.GetNode<MegaLabel>("Amount2Label");
            SetEmphasis(__instance, label2, emphasized.SecondAmountColor, emphasized.ShouldEmphasizeSecondAmount);
        }

        static void SetEmphasis(NPower instance, MegaLabel label, Color baseColor, bool shouldEmphasize)
        {
            bool alreadyOn = _activeTweens.ContainsKey(instance);
            if (shouldEmphasize == alreadyOn)
            {
                if (!shouldEmphasize)
                    label.AddThemeColorOverride(ThemeConstants.Label.FontColor, baseColor);
                return;
            }

            if (shouldEmphasize)
            {
                Color emphasisColor = Colors.Red;
                var tween = label.CreateTween().SetLoops();
                tween.TweenMethod(
                    Callable.From<float>(t => label.AddThemeColorOverride(ThemeConstants.Label.FontColor, baseColor.Lerp(emphasisColor, t))),
                    0.0, 1.0, 1
                ).SetEase(Tween.EaseType.InOut).SetTrans(Tween.TransitionType.Sine);
                tween.TweenMethod(
                    Callable.From<float>(t => label.AddThemeColorOverride(ThemeConstants.Label.FontColor, baseColor.Lerp(emphasisColor, t))),
                    1.0, 0.0, 1
                ).SetEase(Tween.EaseType.InOut).SetTrans(Tween.TransitionType.Sine);
                _activeTweens[instance] = tween;
            }
            else
            {
                _activeTweens[instance].Kill();
                _activeTweens.Remove(instance);
                label.AddThemeColorOverride(ThemeConstants.Label.FontColor, baseColor);
            }
        }

        [HarmonyPatch(typeof(NPower), "_ExitTree")]
        [HarmonyPostfix]
        static void CleanupTween(NPower __instance)
        {
            if (_activeTweens.TryGetValue(__instance, out var tween))
            {
                tween.Kill();
                _activeTweens.Remove(__instance);
            }
        }
    }
}
