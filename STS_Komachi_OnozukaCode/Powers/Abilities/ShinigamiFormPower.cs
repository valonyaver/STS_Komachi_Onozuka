using BaseLib.Abstracts;
using BaseLib.Extensions;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Cards.Tokens;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Commands;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Powers.Distance;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Powers.Abilities
{
    public class ShinigamiFormPower : STS_Komachi_OnozukaPower
    {
        public override PowerType Type => PowerType.Buff;
        public override PowerStackType StackType => PowerStackType.Counter;

        private const int CloseLevelMax = 2;
        private const int FarLevelMin = 4;

        // Far final values are constant regardless of stacks — precompute once.
        private static readonly decimal L4Final = GetAmplifiedAdditive(4, stacks: 1);
        private static readonly decimal L5Final = GetAmplifiedAdditive(5, stacks: 1);

        protected override IEnumerable<DynamicVar> CanonicalVars =>
        [
            new DynamicVar("L1Mult", ToSignedPct(3m)), // placeholders; RefreshDisplayVars overwrites on first AfterPowerAmountChanged
            new DynamicVar("L2Mult", ToSignedPct(2m)),
            new DynamicVar("L4Mult", ToSignedPct(L4Final)),
            new DynamicVar("L5Mult", ToSignedPct(L5Final)),
        ];

        // desiredFinal = 1 + bonus * (1 + stacks), where bonus = baseMultiplier - 1
        // Works for both close (bonus positive) and far (bonus negative, stacks fixed at 1).
        private static decimal GetAmplifiedAdditive(int level, int stacks)
        {
            decimal baseMultiplier = DistancePower.GetDamageMultiplier(level);
            decimal bonus = baseMultiplier - 1m;
            return 1m + bonus * (1 + stacks);
        }

        private static int ToSignedPct(decimal multiplier) => (int)((multiplier - 1m) * 100m);

        private void RefreshDisplayVars()
        {
            int s = Amount;
            DynamicVars["L1Pct"].BaseValue = ToSignedPct(GetAmplifiedAdditive(1, s));
            DynamicVars["L2Pct"].BaseValue = ToSignedPct(GetAmplifiedAdditive(2, s));
            if (s == 1)
            {
                DynamicVars["L4Pct"].BaseValue = ToSignedPct(L4Final);
                DynamicVars["L5Pct"].BaseValue = ToSignedPct(L5Final);
            }
            InvokeDisplayAmountChanged();
        }

        public override async Task AfterPowerAmountChanged(PlayerChoiceContext choiceContext, PowerModel power, decimal amount, Creature? applier, CardModel? cardSource)
        {
            if (power != this) return;
            RefreshDisplayVars();
        }

        public override decimal ModifyDamageMultiplicative(Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource, CardPlay? cardPlay)
        {
            if (!props.IsPoweredAttack()) return 1m;

            decimal multiplier = 1m;

            // Close: player dealing damage to a close enemy — stacks with Amount.
            if (dealer == Owner && target != null)
            {
                int level = DistancePower.GetLevel(target);
                if (level <= CloseLevelMax)
                {
                    decimal baseM = DistancePower.GetDamageMultiplier(level);
                    multiplier *= GetAmplifiedAdditive(level, Amount) / baseM;
                }
            }

            // Far: player receiving damage from a far enemy — always stacks=1, ignores Amount.
            if (target == Owner && dealer != null)
            {
                int level = DistancePower.GetLevel(dealer);
                if (level >= FarLevelMin)
                {
                    decimal baseM = DistancePower.GetDamageMultiplier(level);
                    multiplier *= GetAmplifiedAdditive(level, stacks: 1) / baseM;
                }
            }

            return multiplier;
        }
    }
}
