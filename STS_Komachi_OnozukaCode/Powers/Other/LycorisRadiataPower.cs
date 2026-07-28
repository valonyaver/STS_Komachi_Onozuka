using BaseLib.Abstracts;
using BaseLib.Extensions;
using Godot;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.ValueProps;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Cards.Tokens;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Commands;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Patches.PowerPatches;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Powers.Spirits;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Powers.Other
{
    public class LycorisRadiataPower : STS_Komachi_OnozukaPower, IHasSecondAmount, IHasColoredSecondAmount
    {
        public override PowerType Type => PowerType.Buff;
        public override PowerStackType StackType => PowerStackType.Counter;

        protected override IEnumerable<DynamicVar> CanonicalVars =>
        [
            new IntVar(nameof(PoisonAmount), 0)
        ];

        /// <summary>
        /// Increases the poison application from lycoris radiata. 
        /// Used in the LycorisRadiata class itself rather than here.
        /// </summary>
        public int PoisonAmount
        {
            get => DynamicVars[nameof(PoisonAmount)].IntValue;
            set
            {
                DynamicVars[nameof(PoisonAmount)].BaseValue = value;
                PowerExtensions.InvokeSecondAmountChanged(this);
            }
        }

        public Color SecondAmountColor => StsColors.green;
        public bool ShouldEmphasizeSecondAmount => false;

        public string GetSecondAmount()
        {
            return PoisonAmount.ToString();
        }

        public override decimal ModifyDamageAdditive(Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource, CardPlay? cardPlay)
        {
            if (!props.IsPoweredAttack())
            {
                return 0m;
            }

            if (cardSource == null)
            {
                return 0m;
            }

            if (cardSource is not LycorisRadiata)
            {
                return 0m;
            }

            if (dealer != base.Owner && cardSource.Owner != base.Owner.Player)
            {
                return 0m;
            }

            return Amount;
        }
    }
}
