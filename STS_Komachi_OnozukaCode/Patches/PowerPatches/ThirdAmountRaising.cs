using Godot;
using MegaCrit.Sts2.addons.mega_text;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Patches.PowerPatches
{
    public static class ThirdAmountRaiseController
    {
        static readonly HashSet<object> _raisedModels = [];

        public static void OnPreviewTargetChanged(NCard card, Creature? creature)
        {
            bool changed = _raisedModels.Count > 0;
            _raisedModels.Clear();

            if (creature != null && card.Model != null)
            {
                foreach (PowerModel power in creature.Powers)
                {
                    if (power is IHasThirdAmount hasThird && hasThird.ShouldRaiseThirdAmount(card.Model))
                    {
                        _raisedModels.Add(power);
                        changed = true;
                    }
                }
            }

            if (changed)
                CombatManager.Instance.StateTracker.NotifyCombatStateChanged("ThirdAmountRaise");
        }

        public static void Clear()
        {
            if (_raisedModels.Count == 0) return;
            _raisedModels.Clear();
            CombatManager.Instance.StateTracker.NotifyCombatStateChanged("ThirdAmountRaise");
        }

        public static bool ShouldRaise(object? model) => model != null && _raisedModels.Contains(model);
    }

    internal static class ThirdAmountFloatingLabelController
    {
        static readonly Dictionary<object, MegaLabel> _labels = new();
        static readonly Dictionary<object, float> _raiseProgress = new();
        static readonly Dictionary<object, bool> _raiseTarget = new();
        static readonly Dictionary<object, Tween> _raiseTweens = new();

        static float RaiseDistance => 10f;

        public static void Refresh(NPower powerNode, decimal damage, bool shouldRaise, MegaLabel templateLabel)
        {
            object powerModel = powerNode.Model;

            if (!_labels.TryGetValue(powerModel, out var label) || !GodotObject.IsInstanceValid(label))
            {
                label = (MegaLabel)templateLabel.Duplicate(
                    (int)(Node.DuplicateFlags.Signals | Node.DuplicateFlags.Groups | Node.DuplicateFlags.Scripts | Node.DuplicateFlags.UseInstantiation)
                );
                label.Name = "ThirdAmountFloatingLabel";
                label.UniqueNameInOwner = false;
                label.SetAnchorsPreset(Control.LayoutPreset.TopLeft, keepOffsets: false);
                label.AddThemeColorOverride(ThemeConstants.Label.FontColor, StsColors.red);
                label.SelfModulate = new Color(1f, 1f, 1f, 2f);

                powerNode.AddChild(label, false, Node.InternalMode.Disabled);
                powerNode.MoveChild(label, templateLabel.GetIndex(false));
                _labels[powerModel] = label;
            }

            label.Visible = true;
            label.SetTextAutoSize(damage.ToString("0"));

            int fontSize = label.GetThemeFontSize(ThemeConstants.Label.FontSize, "Label");
            Vector2 offset = new Vector2(-(fontSize + 12), -fontSize);

            if (!_raiseTarget.TryGetValue(powerModel, out bool currentTarget) || currentTarget != shouldRaise)
            {
                _raiseTarget[powerModel] = shouldRaise;
                StartRaiseTween(powerNode, shouldRaise, templateLabel);
            }

            float raise = _raiseProgress.GetValueOrDefault(powerModel, 0f);
            UpdateLabelParentAndPosition(powerNode, templateLabel, label, offset, raise, shouldRaise);
        }

        static void StartRaiseTween(NPower powerNode, bool raised, MegaLabel templateLabel)
        {
            object powerModel = powerNode.Model;

            _raiseTweens.TryGetValue(powerModel, out var existing);
            existing?.Kill();

            float from = _raiseProgress.GetValueOrDefault(powerModel, 0f);
            float to = raised ? 1f : 0f;

            var tween = powerNode.CreateTween();
            tween.TweenMethod(Callable.From<float>(t =>
            {
                _raiseProgress[powerModel] = t;
                if (_labels.TryGetValue(powerModel, out var label) && GodotObject.IsInstanceValid(label) && GodotObject.IsInstanceValid(powerNode) && GodotObject.IsInstanceValid(templateLabel))
                {
                    int fontSize = label.GetThemeFontSize(ThemeConstants.Label.FontSize, "Label");
                    Vector2 offset = new Vector2(-(fontSize + 12), -fontSize);
                    UpdateLabelParentAndPosition(powerNode, templateLabel, label, offset, t, raised);
                }
            }), from, to, 0.15).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Sine);

            _raiseTweens[powerModel] = tween;
        }

        static void UpdateLabelParentAndPosition(NPower powerNode, MegaLabel templateLabel, MegaLabel label, Vector2 offset, float raiseProgress, bool shouldRaise)
        {
            var vfxContainer = NCombatRoom.Instance.CombatVfxContainer;

            if (raiseProgress > 0f || shouldRaise)
            {
                // Hovered/Raising: Float on top overlay
                if (label.GetParent() != vfxContainer)
                {
                    label.Reparent(vfxContainer, keepGlobalTransform: true);
                }
                label.GlobalPosition = templateLabel.GlobalPosition + offset + Vector2.Up * (RaiseDistance * raiseProgress);
            }
            else
            {
                // Normal: Childed locally to NPower
                if (label.GetParent() != powerNode)
                {
                    label.Reparent(powerNode, keepGlobalTransform: false);
                    powerNode.MoveChild(label, templateLabel.GetIndex(false));
                }
                label.Position = templateLabel.Position + offset;
            }
        }

        public static void Clear(object powerModel)
        {
            if (_labels.TryGetValue(powerModel, out var label))
            {
                if (GodotObject.IsInstanceValid(label))
                    label.QueueFreeSafely();
                _labels.Remove(powerModel);
            }
            _raiseProgress.Remove(powerModel);
            _raiseTarget.Remove(powerModel);
            _raiseTweens.Remove(powerModel);
        }
    }
}
