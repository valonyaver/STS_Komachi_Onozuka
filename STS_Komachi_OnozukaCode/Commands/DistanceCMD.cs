using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Cards.Tokens;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Powers.Distance;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Commands
{
    public class DistanceChangedEventArgs
    {
        public Creature Target;
        public PowerModel? Power;
        public CardModel? cardSource;
        public int OldLevel;
        public int NewLevel;
        public int Change => NewLevel - OldLevel;
        public int ChangeAbs => Math.Abs(NewLevel - OldLevel);
    }

    public interface IOnDistanceChangedListener
    {
        Task OnDistanceChanged(PlayerChoiceContext choiceContext, DistanceChangedEventArgs args);
    }

    public static class DistanceCmd
    {
        /// <summary>
        /// Changes a creature's Distance level by `levelChange`, clamped to [1, 5].
        /// A creature with no DistancePower is treated as starting at level 3.
        /// This sets Amount to the final absolute level directly rather than going through
        /// PowerCmd.Apply's normal additive Counter-stacking, since Amount here represents
        /// an absolute level, not an accumulating count.
        /// </summary>
        public static async Task<DistanceChangedEventArgs> Displace(PlayerChoiceContext choiceContext, Creature target, int levelChange, Creature? applier = null, CardModel? cardSource = null)
        {
            DistancePower? power = target.GetPower<DistancePower>();
            int oldLevel = power?.Amount ?? DistancePower.DefaultLevel;
            int newLevel = Math.Clamp(oldLevel + levelChange, DistancePower.MinLevel, DistancePower.MaxLevel);

            DistanceChangedEventArgs args = new() { Target = target, Power = power, OldLevel = oldLevel, NewLevel = newLevel, cardSource = cardSource };

            // Nothing ever happens
            if (oldLevel == newLevel)
            {
                return args;
            }

            if (power == null)
            {
                // First application: Amount starts at 0, so Apply just sets it straight to newLevel
                await PowerCmd.Apply<DistancePower>(choiceContext, target, newLevel, applier, cardSource);
            }
            else
            {
                // Already exists: set the absolute value directly instead of adding through PowerCmd.
                power.Amount = newLevel;
            }

            await KomachiHooks.OnDistanceChanged(choiceContext, args);
            return args;
        }

        /// <summary>
        /// Builds a "choose a displacement" screen from raw level-change values, 
        /// relative to the target's current distance level.
        /// 0 → an explicit "do nothing" token (ManipulateNoDistanceToken).
        /// Negative n → a pull token (AltDescription 1, Value1 = |n|).
        /// Positive n → a push token (AltDescription 2, Value1 = n).
        /// Order of `options` is the order tokens appear on the choice screen.
        /// </summary>
        public static async Task<CardModel?> ChooseDisplacement(PlayerChoiceContext choiceContext, Creature target, CardModel card, params int[] options)
        {
            if (card.CombatState == null) return null;
            if (target.IsDead) return null;
            int currentLevel = DistancePower.GetLevel(target);

            // First pass: for each resulting level, find the option with the smallest magnitude
            // that reaches it. E.g. at level 2 with options {-2, -1, 0}: -2 and -1 both resolve
            // to level 1, but -1 is the more exact/direct move, so it wins over -2.
            var bestForLevel = new Dictionary<int, int>();
            foreach (int n in options)
            {
                int resultLevel = Math.Clamp(currentLevel + n, DistancePower.MinLevel, DistancePower.MaxLevel);
                if (!bestForLevel.TryGetValue(resultLevel, out int existing) || Math.Abs(n) < Math.Abs(existing))
                {
                    bestForLevel[resultLevel] = n;
                }
            }

            // Second pass: rebuild in original order, keeping only each level's winning option,
            // and only the first time we hit it (in case the same n appears twice in options).
            var addedLevels = new HashSet<int>();
            List<int> dedupedOptions = [];
            foreach (int n in options)
            {
                int resultLevel = Math.Clamp(currentLevel + n, DistancePower.MinLevel, DistancePower.MaxLevel);
                if (bestForLevel[resultLevel] == n && addedLevels.Add(resultLevel))
                {
                    dedupedOptions.Add(n);
                }
            }

            // 1. If all options resolve to no distance change, do nothing.
            if (dedupedOptions.Count == 0 || (dedupedOptions.Count == 1 && dedupedOptions[0] == 0))
            {
                return null;
            }

            // 2. If exactly one valid displacement option remains, return the token directly without showing UI.
            if (dedupedOptions.Count == 1)
            {
                return CreateDisplacementToken(card, dedupedOptions[0]);
            }

            List<CardModel> list = [];
            foreach (int n in dedupedOptions)
            {
                list.Add(CreateDisplacementToken(card, n));
            }

            if (list.Count > 3)
            {
                return (await CardSelectCmd.FromSimpleGrid(
                    choiceContext, list, card.Owner,
                    new CardSelectorPrefs(DistancePower.StaticSelectionScreenPrompt, 1)
                    )).FirstOrDefault();
            }
            return await CardSelectCmd.FromChooseACardScreen(choiceContext, list, card.Owner, false);
        }

        /// <summary>
        /// Helper method to build token
        /// </summary>
        private static CardModel CreateDisplacementToken(CardModel card, int n)
        {
            CardModel finalToken;
            if (n == 0)
            {
                ManipulateNoDistanceToken token = card.CombatState.CreateCard<ManipulateNoDistanceToken>(card.Owner);
                token.AltDescription = 0;
                finalToken = token;
            }
            else if (n < 0)
            {
                ManipulateDistanceToken token = card.CombatState.CreateCard<ManipulateDistanceToken>(card.Owner);
                token.AltDescription = 1;
                token.Value1 = -n;
                token.ExtraDescription1 = token.RawExtraDescription1.GetFormattedText();
                finalToken = token;
            }
            else
            {
                ManipulateDistanceToken token = card.CombatState.CreateCard<ManipulateDistanceToken>(card.Owner);
                token.AltDescription = 2;
                token.Value1 = n;
                token.ExtraDescription2 = token.RawExtraDescription2.GetFormattedText();
                finalToken = token;
            }
            MainFile.LogMessage($"Generating a mandist with a number of {n}. Its value1 is {finalToken.DynamicVars["Value1"].IntValue}");
            return finalToken;
        }

        /// <summary>
        /// Shortcut: shows the choice screen, then immediately applies the resulting displacement.
        /// Returns null if the player skipped or picked the "do nothing" token.
        /// Should probably add a condition to not add redundant options
        /// </summary>
        public static async Task<DistanceChangedEventArgs?> ChooseAndDisplace(PlayerChoiceContext choiceContext, Creature target, CardModel card, params int[] options)
        {
            CardModel? chosen = await ChooseDisplacement(choiceContext, target, card, options);
            if (chosen != null && chosen is ManipulateDistanceToken token && token.AltDescription != 0)
            {
                int displacementValue = token.AltDescription == 1 ? -token.Value1 : token.Value1;
                return await Displace(choiceContext, target, displacementValue, card.Owner.Creature, card);
            }
            return null;
        }

        /// <summary>
        /// Pick an absolute Distance level rather than a relative shift.
        /// </summary>
        public static async Task<DistanceChangedEventArgs> DisplaceTo(PlayerChoiceContext choiceContext, Creature target, int absoluteLevel, Creature? applier = null, CardModel? cardSource = null)
        {
            int currentLevel = DistancePower.GetLevel(target);
            return await Displace(choiceContext, target, absoluteLevel - currentLevel, applier, cardSource);
        }

        /// <summary>
        /// Returns the result of a potential displacement on a target.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="levelChange"></param>
        /// <returns></returns>
        public static int PredictLevel(Creature target, int levelChange)
        {
            int current = DistancePower.GetLevel(target);
            return Math.Clamp(current + levelChange, DistancePower.MinLevel, DistancePower.MaxLevel);
        }
    }
}
