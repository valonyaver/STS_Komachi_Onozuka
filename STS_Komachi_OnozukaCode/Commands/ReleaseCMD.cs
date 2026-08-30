using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Cards;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Cards.Tokens;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Powers.Abilities;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Powers.Spirits;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Commands
{
    public class ReleaseArgs
    {
        /// <summary>
        /// Original amount of spirits to be release, in case they get changed by Eiki
        /// </summary>
        public int originalReleaseAmount;
        /// <summary>
        /// Amount of spirits to be release
        /// </summary>
        public int ReleaseAmount;


        /// <summary>
        /// The virtual amount of Guided Spirits that would be released (ignores Eiki's free release)
        /// </summary>
        public int IntendedGuidedReleaseAmount;
        /// <summary>
        /// The virtual amount of Divine Spirits that would be released (ignores Eiki's free release)
        /// </summary>
        public int IntendedDivineReleaseAmount;
        /// <summary>
        /// How many Guided spirits were released by this command
        /// </summary>
        public int GuidedSpiritReleaseAmount;
        /// <summary>
        /// How many Guided spirits were released by this command
        /// </summary>
        public int DivineSpiritReleaseAmount;
        /// <summary>
        /// If the release was successful. Release goes through a CanReleaseSpirits check. If it can't do it, then it's unsuccessful.
        /// </summary>
        public bool Successful;
        /// <summary>
        /// If either guided or divine spirits were removed completely from this.
        /// </summary>
        public bool RemovedCompletely;
        /// <summary>
        /// Creature/player that is releasing the spirits
        /// </summary>
        public Creature creature;
    }

    public interface IOnReleasingListener
    {
        Task OnReleasing(PlayerChoiceContext choiceContext, ReleaseArgs args);
    }

    public interface IOnReleasedListener
    {
        Task OnReleased(PlayerChoiceContext choiceContext, ReleaseArgs args);
    }

    public static class ReleaseCmd
    {
        /// <summary>
        /// Removes `amount` total spirits from `unit`, Guided Spirits first, then Divine Spirits
        /// for whatever remains. Caller should gate this behind CanReleaseSpirits beforehand
        /// (ChooseRelease already does).
        /// </summary>
        public static async Task<ReleaseArgs> Release(PlayerChoiceContext choiceContext, Creature unit, int amount, CardModel? cardSource = null)
        {
            ReleaseArgs args = new() { originalReleaseAmount = amount, ReleaseAmount = amount, creature = unit };

            // Calculate Intended amounts BEFORE hooks modify the release behavior.
            int currentGuided = unit.GetPower<GuidedSpiritPower>()?.Amount ?? 0;
            int currentDivine = unit.GetPower<DivineSpiritPower>()?.Amount ?? 0;

            args.IntendedGuidedReleaseAmount = Math.Min(currentGuided, amount);
            args.IntendedDivineReleaseAmount = Math.Min(currentDivine, amount - args.IntendedGuidedReleaseAmount);

            await KomachiHooks.OnReleasing(choiceContext, args);

            if (!CanReleaseSpirits(unit, amount))
            {
                return args;
            }

            args.Successful = true;
            int remaining = args.ReleaseAmount;

            GuidedSpiritPower? guidedSpirits = unit.GetPower<GuidedSpiritPower>();
            if (guidedSpirits != null && guidedSpirits.Amount > 0)
            {
                int guidedToRelease = Math.Min(guidedSpirits.Amount, remaining);
                await PowerCmd.ModifyAmount(choiceContext, guidedSpirits, -guidedToRelease, unit, cardSource);
                remaining -= guidedToRelease;
                args.GuidedSpiritReleaseAmount = guidedToRelease;
            }

            DivineSpiritPower? divineSpirits = unit.GetPower<DivineSpiritPower>();
            if (remaining > 0 && divineSpirits != null && divineSpirits.Amount > 0)
            {
                int divineToRelease = Math.Min(divineSpirits.Amount, remaining);
                await PowerCmd.ModifyAmount(choiceContext, divineSpirits, -divineToRelease, unit, cardSource);
                args.DivineSpiritReleaseAmount = divineToRelease;
            }

            await KomachiHooks.OnReleased(choiceContext, args);

            args.RemovedCompletely = args.GuidedSpiritReleaseAmount == amount || args.DivineSpiritReleaseAmount == amount;
            return args;
        }

        public static bool CanReleaseSpirits(Creature unit, int requiredAmount)
        {
            if (unit.HasPower<EikiFreeReleasePower>()) return true;
            int guided = unit.GetPower<GuidedSpiritPower>()?.Amount ?? 0;
            int divine = unit.GetPower<DivineSpiritPower>()?.Amount ?? 0;
            return guided + divine >= requiredAmount;
        }

        /// <summary>
        /// Builds and shows the choice screen: don't release, release at cost1, and (if affordable) release at cost2.
        /// Returns null only if the player can't even afford cost1, meaning the card shouldn't have offered a release at all.
        /// </summary>
        public static async Task<CardModel?> ChooseRelease(PlayerChoiceContext choiceContext, CardModel card, int cost1, int cost2 = 0)
        {
            Creature player = card.Owner.Creature;

            if (card.CombatState == null || !CanReleaseSpirits(player, cost1))
            {
                return null;
            }

            ReleaseToken releaseNone = card.CombatState.CreateCard<ReleaseToken>(card.Owner);
            releaseNone.AltDescription = 0;

            ReleaseToken releaseCost1 = card.CombatState.CreateCard<ReleaseToken>(card.Owner);
            releaseCost1.AltDescription = 1;
            releaseCost1.ExtraDescription1 = ((STS_Komachi_OnozukaCard)card).RawExtraDescription1.GetFormattedText();

            List<CardModel> options = [releaseNone, releaseCost1];

            if (cost2 > 0 && CanReleaseSpirits(player, cost2))
            {
                ReleaseToken releaseCost2 = card.CombatState.CreateCard<ReleaseToken>(card.Owner);
                releaseCost2.AltDescription = 2;
                releaseCost2.ExtraDescription2 = ((STS_Komachi_OnozukaCard)card).RawExtraDescription2.GetFormattedText();
                options.Add(releaseCost2);
            }

            return await CardSelectCmd.FromChooseACardScreen(choiceContext, options, card.Owner, canSkip: false);
        }

        public static bool ChoseRelease(CardModel? choiceCard)
        {
            return choiceCard != null && choiceCard is ReleaseToken token && token.AltDescription != 0;
        }

        /// <summary>
        /// Shortcut for the 2-cost case: tells you whether a release happened and which cost applies.
        /// </summary>
        public static bool ChoseRelease(CardModel? choiceCard, int cost1, int cost2, out int costResult)
        {
            if (!ChoseRelease(choiceCard))
            {
                costResult = 0;
                return false;
            }

            costResult = ((ReleaseToken)choiceCard!).AltDescription == 1 ? cost1 : cost2;
            return true;
        }

        /// <summary>
        /// If the chosen token card has an alt description of 2, ie it's the second release choice.
        /// </summary>
        /// <param name="choiceCard"></param>
        /// <returns></returns>
        public static bool ChoseSecondRelease(CardModel? choiceCard)
        {
            return choiceCard is ReleaseToken token && token.AltDescription == 2;
        }
    }
}
