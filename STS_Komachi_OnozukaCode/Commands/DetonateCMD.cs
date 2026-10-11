using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Powers.Abilities;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Powers.Spirits;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static HarmonyLib.Code;

namespace STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Commands
{
    public class DetonationEventArgs
    {
        public Creature Target;
        public PowerModel DetonatingPower;
        public CardModel? CardSource;
        public Creature? Dealer;
        public decimal Multiplier = 2m;

        public bool DetonatedByEffect => CardSource != null;

        // Set true to fizzle the detonation entirely. No current power needs this,
        // but the old "-ing" event existing at all implies something eventually will.
        public bool Succcessful;

        /// <summary>
        /// Original Amount of the detonating power. Used for calculating the damage.
        /// </summary>
        public decimal OriginalAmount;
        /// <summary>
        /// Actual damage dealt from the detonation. Set in Detonated.
        /// </summary>
        public DamageResult damageResult;

        /// <summary>
        /// Additional amount of spirits detonated for card effect purposes. E.G. Boom Benefits.
        /// It is added in from lonely bound spirits.
        /// </summary>
        public decimal BonusAmount;

        /// <summary>
        /// Total amount of Spirits detonated. Used for card effects that care about how many spirits were detonated at the same time.
        /// </summary>
        public decimal TotalCountedAmount => OriginalAmount + BonusAmount;
    }

    /// <summary>
    /// Hooks into detonations to do stuff before the detonation happens.
    /// </summary>
    public interface IOnDetonatingListener
    {
        Task OnDetonating(PlayerChoiceContext choiceContext, DetonationEventArgs args);
    }

    /// <summary>
    /// Like OnDetonatedListener, but runs before it.
    /// </summary>
    public interface IOnDetonatedEarlyListener
    {
        Task OnDetonatedEarly(PlayerChoiceContext choiceContext, DetonationEventArgs args);
    }

    /// <summary>
    /// Hooks into detonations to do stuff after the detonation happens.
    /// </summary>
    public interface IOnDetonatedListener
    {
        Task OnDetonated(PlayerChoiceContext choiceContext, DetonationEventArgs args);
    }

    public static class DetonateCmd
    {
        /// <summary>
        /// Safe entry point — fizzles (no-op, returns null) if target has no VengefulSpiritPower.
        /// Use this from cards/effects targeting an arbitrary enemy.
        /// </summary>
        /// <returns></returns>
        public static async Task<DetonationEventArgs?> Target(PlayerChoiceContext choiceContext, Creature target, CardModel? cardSource = null)
        {
            VengefulSpiritPower? vs = target.GetPower<VengefulSpiritPower>();
            if (vs == null)
            {
                LonelyBoundSpiritPower? lbs = target.GetPower<LonelyBoundSpiritPower>();

                if (lbs == null) return null;


                return await Detonate(choiceContext, lbs, cardSource);
            }

            return await Detonate(choiceContext, vs, cardSource);
        }

        /// <summary>
        /// Normal detonate command for detonating VengefulSpiritPower.
        /// </summary>
        /// <param name="choiceContext"></param>
        /// <param name="power"></param>
        /// <param name="cardSource"></param>
        /// <returns></returns>
        public static Task<DetonationEventArgs> Detonate(PlayerChoiceContext choiceContext, PowerModel power, CardModel? cardSource = null)
        {
            return DetonateRaw(choiceContext, power, power.Amount, power.Applier, cardSource);
        }

        /// <summary>
        /// Lower-level entry point for powers whose explosion size isn't their own Amount
        /// (e.g. LonelyBoundSpiritPower explodes off its accumulated Count, not its stack count).
        /// </summary>
        public static async Task<DetonationEventArgs> DetonateRaw(PlayerChoiceContext choiceContext, PowerModel power, decimal explosionAmount, Creature? dealer, CardModel? cardSource)
        {
            DetonationEventArgs args = new()
            {
                Target = power.Owner,
                DetonatingPower = power,
                CardSource = cardSource,
                Dealer = dealer,
                OriginalAmount = explosionAmount
            };

            await KomachiHooks.OnDetonating(choiceContext, args);

            if (!args.Target.Powers.Contains(power))
            {
                args.Succcessful = false;
                return args;
            }


            args.Succcessful = true;

            if (power is VengefulSpiritPower vengefulSpirit)
            {
                vengefulSpirit.currentDetonationArgs = args;
            }
            else if (power is LonelyBoundSpiritPower lonelySpirit)
            {
                lonelySpirit.currentDetonationArgs = args;
            }

            await PowerCmd.Remove(power);

            await KomachiHooks.OnDetonatedEarly(choiceContext, args);
            await KomachiHooks.OnDetonated(choiceContext, args);

            return args;
        }

        public static bool IsDetonationSuccessful(DetonationEventArgs args)
        {
            return args != null && args.Succcessful;
        }
    }
}
