using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.ValueProps;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Cards;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Powers.Distance
{
    public class DistancePower : STS_Komachi_OnozukaPower
    {

        public static LocString StaticSelectionScreenPrompt
        {
            get
            {
                LocString locString = new LocString("powers", "STS_KOMACHI_ONOZUKA-DISTANCE_POWER.selectionScreenPrompt");
                if (!locString.Exists())
                {
                    throw new InvalidOperationException($"No selection screen prompt for DistancePower.");
                }

                return locString;
            }
        }
        public override PowerType Type => PowerType.None;
        public override PowerStackType StackType => PowerStackType.Counter;

        public const int MinLevel = 1;
        public const int MaxLevel = 5;
        public const int DefaultLevel = 3;

        /// <summary>
        /// Returns the default damage multipliers for any particular level.
        /// </summary>
        public static decimal GetDamageMultiplier(int level)
        {
            return level switch
            {
                1 => 2.0m,  // Very Close: +100%
                2 => 1.5m,  // Close: +50%
                3 => 1.0m,  // Normal: no change
                4 => 0.85m, // Far: -15%
                5 => 0.7m,  // Very Far: -30%
                _ => 1.0m
            };
        }

        /// <summary>
        /// Gets the inverse of the damage multiplier times 2. Used for Scythe of Final Judgement.
        /// </summary>
        public static decimal GetInverseDamageMultiplier(int level)
        {
            decimal standard = GetDamageMultiplier(level);
            return 1m + (1m - standard) * 2m;
        }

        /// <summary>
        /// Returns a creature's current Distance level, or DefaultLevel (3) if it has no DistancePower yet.
        /// </summary>
        public static int GetLevel(Creature creature)
        {
            return creature.GetPower<DistancePower>()?.Amount ?? DefaultLevel;
        }

        public int? PreviewAmountOverride;
        int EffectiveAmount => PreviewAmountOverride ?? Amount;
        public override decimal ModifyDamageMultiplicative(Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource, CardPlay? cardPlay)
        {
            // Ignore if normal attack
            if (!props.IsPoweredAttack())
            {
                return 1m;
            }

            decimal multiplier = 1m;

            // Owner is taking this hit — applies the "takes" half of the table.
            if (target == Owner)
            {
                multiplier *= GetEffectiveMultiplier(EffectiveAmount, cardSource);
            }

            // Owner is dealing this hit — applies the "deals" half of the table.
            if (dealer == Owner)
            {
                multiplier *= GetEffectiveMultiplier(EffectiveAmount, cardSource);
            }

            return multiplier;
        }

        /// <summary>
        /// Komachi cards can override what multipliers are applied at certain distance levels. 
        /// This function takes that into account, and otherwise returns the normal damage multipliers.
        /// </summary>
        static decimal GetEffectiveMultiplier(int level, CardModel? cardSource)
        {
            if (cardSource is STS_Komachi_OnozukaCard komachiCard)
            {
                decimal? overrideMult = komachiCard.GetDistanceMultiplierOverride(level);
                if (overrideMult.HasValue) return overrideMult.Value;
            }
            return GetDamageMultiplier(level);
        }

        #region Preview Stuff
        public readonly record struct DisplacementPreview(int CurrentLevel, int[] ReachableLevels, IReadOnlyDictionary<int, decimal> DamageByLevel);

        public static DisplacementPreview? PreviewDisplacementDamage(CardModel card, Creature target)
        {
            // Skips trying to preview damage for non komachi cards
            if (card is not STS_Komachi_OnozukaCard komachiCard) return null;

            // Gets the possible displacements of a card. Skips if it doesn't have any.
            int[]? deltas = komachiCard.GetPossibleDisplacements();
            if (deltas == null || deltas.Length == 0) return null;
            // Get damage of the card.
            if (!card.DynamicVars.TryGetValue("Damage", out DynamicVar? damageVar)) return null;

            // Get current level of the target.
            int currentLevel = GetLevel(target);

            // Finds all the reachable levels depending on the deltas of the card.
            // Clamps and makes unique all values that would to below or above 1 or 5.
            int[] reachableLevels = deltas
                .Select(d => Math.Clamp(currentLevel + d, MinLevel, MaxLevel))
                .Distinct()
                .OrderBy(l => l)
                .ToArray();

            // Current preview damage of the card.
            decimal currentPreview = damageVar.PreviewValue;
            // Finds the current multiplier of the card on the target's distance.
            decimal currentMultiplier = GetEffectiveMultiplier(currentLevel, card);
            if (currentMultiplier == 0m) return null;

            // Compute the damage number on all possible distance evels.
            var damageByLevel = new Dictionary<int, decimal>(5);
            for (int level = MinLevel; level <= MaxLevel; level++)
            {
                // Multiplier of calculated level.
                decimal hypothetical = GetEffectiveMultiplier(level, card);
                // divides current preview damage by current multiplier, then multiplies by the calculated multiplier.
                damageByLevel[level] = Math.Floor(currentPreview * hypothetical / currentMultiplier);
            }

            return new DisplacementPreview(currentLevel, reachableLevels, damageByLevel);

        }
        public readonly record struct EnemyIntentDistancePreview(int CurrentLevel, IReadOnlyDictionary<int, decimal> DamageByLevel);

        public static EnemyIntentDistancePreview? PreviewIntentDamageAcrossDistances(Creature creature, IReadOnlyList<Creature> targets)
        {
            DistancePower? power = creature.GetPower<DistancePower>();
            if (power == null) return null;
            MonsterModel? monster = creature.Monster;
            if (monster == null || !monster.IntendsToAttack) return null;

            var attackIntents = monster.NextMove.Intents.OfType<AttackIntent>().ToList();
            if (attackIntents.Count == 0) return null;

            int currentLevel = power.Amount;
            var damageByLevel = new Dictionary<int, decimal>(5);
            try
            {
                for (int level = MinLevel; level <= MaxLevel; level++)
                {
                    power.PreviewAmountOverride = level;
                    // Re-runs the REAL GetTotalDamage → GetSingleDamage → Hook.ModifyDamage chain,
                    // so per-hit flooring + repeat multiplication happen exactly as they would in
                    // an actual attack — no rescaling, no compounded rounding.
                    damageByLevel[level] = attackIntents.Sum(intent => intent.GetTotalDamage(targets, creature));
                }
            }
            finally
            {
                power.PreviewAmountOverride = null; // guaranteed even if GetTotalDamage throws
            }
            return new EnemyIntentDistancePreview(currentLevel, damageByLevel);
        }

        #endregion
    }
}
