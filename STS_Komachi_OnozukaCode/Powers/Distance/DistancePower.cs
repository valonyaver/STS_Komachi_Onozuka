using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.ValueProps;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Cards;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Character;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Configs;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Patches.PowerPatches;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Powers.Abilities;
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
            DistancePower? power = creature.GetPower<DistancePower>();
            if (power == null) return DefaultLevel;

            // FIX: Use PreviewAmountOverride if it exists, otherwise fall back to Amount
            return power.EffectiveAmount;
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
            // Does it have any damage var (DamageVar, CalculatedDamageVar, OstyDamageVar)?
            DynamicVar? damageVar = GetDamageVar(card);
            if (damageVar == null) return null;

            // Is distance active/relevant in this combat?
            if (!IsDistanceRelevant(target, KomachiConfigs.ShowNoDistanceCardDamagePreview)) return null;

            // Get current level of the target.
            int currentLevel = GetLevel(target);

            // Only Komachi cards can displace; non-Komachi cards return null for deltas.
            int[]? deltas = (card as STS_Komachi_OnozukaCard)?.GetPossibleDisplacements();

            // Finds all reachable levels depending on displacement deltas.
            // If it doesn't displace (or isn't a Komachi card), only currentLevel is reachable.
            int[] reachableLevels = deltas?
                .Select(d => Math.Clamp(currentLevel + d, MinLevel, MaxLevel))
                .Distinct()
                .OrderBy(l => l)
                .ToArray() ?? [currentLevel];

            // Current preview damage of the card.
            decimal currentPreview = damageVar.PreviewValue;

            // Finds the current multiplier of the card on the target's distance.
            decimal currentMultiplier = GetCardAttackMultiplier(currentLevel, card);
            if (currentMultiplier == 0m) return null;

            // Compute the damage number on all possible distance levels.
            var damageByLevel = new Dictionary<int, decimal>(5);
            for (int level = MinLevel; level <= MaxLevel; level++)
            {
                decimal hypothetical = GetCardAttackMultiplier(level, card);
                damageByLevel[level] = Math.Floor(currentPreview * hypothetical / currentMultiplier);
            }

            return new DisplacementPreview(currentLevel, reachableLevels, damageByLevel);
        }

        /// <summary>
        /// Finds the active damage variable for a card, supporting DamageVar, 
        /// CalculatedDamageVar, and OstyDamageVar, while ignoring ExtraDamageVar.
        /// </summary>
        public static DynamicVar? GetDamageVar(CardModel card)
        {
            // Fast path: check standard keys first
            if (card.DynamicVars.TryGetValue("Damage", out DynamicVar? damage) && damage is not ExtraDamageVar)
            {
                return damage;
            }

            if (card.DynamicVars.TryGetValue("CalculatedDamage", out DynamicVar? calculated))
            {
                return calculated;
            }

            if (card.DynamicVars.TryGetValue("OstyDamage", out DynamicVar? osty))
            {
                return osty;
            }

            // 2. Fallback: Search by type in case the card used a custom variable name
            foreach (DynamicVar v in card.DynamicVars.Values)
            {
                if (v is DamageVar or CalculatedDamageVar or OstyDamageVar)
                {
                    return v;
                }
            }

            return null;
        }

        public static DisplacementPreview? PreviewSpiritDamage(PowerModel power, Creature target, Creature dealer)
        {
            if (power is not IHasThirdAmount hasThird) return null;

            decimal? currentDamage = hasThird.GetThirdAmount();
            if (currentDamage == null || currentDamage.Value <= 0) return null;

            if (!IsDistanceRelevant(target, KomachiConfigs.ShowNoDistanceCardDamagePreview)) return null;

            int currentLevel = GetLevel(target);
            decimal currentMultiplier = GetAttackMultiplier(currentLevel, dealer);
            if (currentMultiplier == 0m) return null;

            var damageByLevel = new Dictionary<int, decimal>(5);
            for (int level = MinLevel; level <= MaxLevel; level++)
            {
                decimal hypothetical = GetAttackMultiplier(level, dealer);
                damageByLevel[level] = Math.Floor(currentDamage.Value * hypothetical / currentMultiplier);
            }

            // Only the target's current distance is highlighted; no displacement is happening.
            return new DisplacementPreview(currentLevel, [currentLevel], damageByLevel);
        }
        public readonly record struct EnemyIntentDistancePreview(int CurrentLevel, IReadOnlyDictionary<int, decimal> DamageByLevel);

        public static EnemyIntentDistancePreview? PreviewIntentDamageAcrossDistances(Creature creature, IReadOnlyList<Creature> targets)
        {
            // 1. Relevance check (checks config and whether distance is active in combat)
            if (!IsDistanceRelevant(creature, KomachiConfigs.ShowNoDistanceEnemyDamagePreview)) return null;

            MonsterModel? monster = creature.Monster;
            if (monster == null || !monster.IntendsToAttack) return null;

            var attackIntents = monster.NextMove.Intents.OfType<AttackIntent>().ToList();
            if (attackIntents.Count == 0) return null;

            DistancePower? power = creature.GetPower<DistancePower>();

            // 2. Resolve the local player on THIS client's screen
            Player? me = LocalContext.GetMe(creature.CombatState);

            // 3. Does Shinigami Form apply to ME for this attack?
            bool applyShinigami = me != null
                && targets.Contains(me.Creature)
                && me.Creature.HasPower<ShinigamiFormPower>();

            // Target to preview against (specifically evaluate against the local player if present)
            IReadOnlyList<Creature> localTarget = me != null ? [me.Creature] : targets;

            int currentLevel = power?.Amount ?? DefaultLevel;
            var damageByLevel = new Dictionary<int, decimal>(5);

            // CASE A: Power already exists on enemy
            if (power != null)
            {
                try
                {
                    for (int level = MinLevel; level <= MaxLevel; level++)
                    {
                        power.PreviewAmountOverride = level;
                        damageByLevel[level] = attackIntents.Sum(intent => intent.GetTotalDamage(localTarget, creature));
                    }
                }
                finally
                {
                    power.PreviewAmountOverride = null;
                }
            }
            // CASE B: Turn 1 (Power is null, implicitly Level 3)
            else
            {
                for (int level = MinLevel; level <= MaxLevel; level++)
                {
                    decimal multiplier = GetDamageMultiplier(level);
                    if (applyShinigami && level >= 4)
                        multiplier = ShinigamiFormPower.GetAmplifiedAdditive(level, stacks: 1);

                    decimal total = 0m;
                    foreach (AttackIntent intent in attackIntents)
                    {
                        int single = intent.GetSingleDamage(localTarget, creature); // per-hit, already truncated
                        int hits = Math.Max(1, intent.Repeats);
                        total += Math.Floor(single * multiplier) * hits;
                    }
                    damageByLevel[level] = total;
                }
            }

            return new EnemyIntentDistancePreview(currentLevel, damageByLevel);
        }

        /// <summary>
        /// Is distance relevant to showing the previews on this enemy?
        /// </summary>
        /// <param name="enemy"></param>
        /// <param name="allowWithoutDistancePower">Configs get passed to this. Should previews always show up even if the enemy has no distance power?</param>
        /// <returns></returns>
        public static bool IsDistanceRelevant(Creature? enemy, bool allowWithoutDistancePower)
        {
            if (enemy == null) return false;

            // If the enemy already has Distance applied, it's always relevant
            if (enemy.GetPower<DistancePower>() != null)
            {
                return true;
            }
            // If configs don't allow previews without the power, say no.
            if (!allowWithoutDistancePower)
            {
                return false;
            }

            /// Only check if *I* (the local player) use distance.
            Player? me = LocalContext.GetMe(enemy.CombatState);
            return me != null && IsPlayerUsingDistance(me);
        }
        /// <summary>
        /// If the player is komachi, or they have a distance card in deck, then distance is relevant.
        /// </summary>
        private static bool IsPlayerUsingDistance(Player player)
        {
            // Player is playing Komachi
            if (player.Character is Komachi_Character)
            {
                return true;
            }

            // Player has a distance card in their combat deck
            if (player.PlayerCombatState != null)
            {
                return player.PlayerCombatState.AllCards.Any(c =>
                    c is STS_Komachi_OnozukaCard komachi &&
                    (komachi.GetPossibleDisplacements()?.Length > 0));
            }

            return false;
        }

        public static decimal GetAttackMultiplier(int level, Creature? dealer, CardModel? card = null)
        {
            decimal baseMultiplier = GetEffectiveMultiplier(level, card);

            // If the dealer has Shinigami Form, amplify Lv 1 & 2
            var shinigami = dealer?.GetPower<ShinigamiFormPower>();
            if (shinigami != null && level <= 2)
            {
                decimal standardBase = GetDamageMultiplier(level);
                return baseMultiplier * (ShinigamiFormPower.GetAmplifiedAdditive(level, shinigami.Amount) / standardBase);
            }

            return baseMultiplier;
        }

        public static decimal GetCardAttackMultiplier(int level, CardModel card) =>
            GetAttackMultiplier(level, card.Owner?.Creature, card);
        #endregion
    }
}
