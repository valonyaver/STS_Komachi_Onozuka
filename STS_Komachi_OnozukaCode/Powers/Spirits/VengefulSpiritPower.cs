using BaseLib.Abstracts;
using BaseLib.Extensions;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Badges;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Commands;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Extras;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Patches.PowerPatches;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Patches.Previewers;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Powers.Abilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Powers.Spirits
{
    public class VengefulSpiritPower : STS_Komachi_OnozukaPower, IHasSecondAmount, IHasEmphasizedSecondAmount, IPreExtraHoverTips, IHasAmbientDamagePreview
    {
        public override PowerType Type => PowerType.Debuff;
        public override PowerStackType StackType => PowerStackType.Counter;
        public override PowerInstanceType InstanceType => PowerInstanceType.InstancedPerApplier;
        public override Color AmountLabelColor => StsColors.purple;
        protected override IEnumerable<DynamicVar> CanonicalVars =>
        [
            new IntVar(nameof(Duration), 3),
            new DamageVar(nameof(VengefulDamage), 2m, ValueProp.Move)
        ];


        public Creature? DamageTarget => Owner; // explodes onto its own owner
        /// <summary>
        /// The base un-modified explosion damage (Amount * 2).
        /// </summary>
        public decimal BaseDamage => Amount * 2m;
        /// <summary>
        /// Previews what the damage should be against the damage target.
        /// </summary>
        public decimal ModifiedDamage => SpiritDamageHelper.FindDamageDealt(Applier, DamageTarget, BaseDamage, DynamicVars[nameof(VengefulDamage)]);

        /// <summary>
        /// Same as modified damage, but used to get the localization.
        /// Returns the base damage though, since FindDamageDealt only updates the preview amount.
        /// </summary>
        public decimal VengefulDamage
        {
            get
            {
                SpiritDamageHelper.FindDamageDealt(Applier, DamageTarget, BaseDamage, DynamicVars[nameof(VengefulDamage)]);
                return DynamicVars[nameof(VengefulDamage)].IntValue;
            }
        }
        public decimal? GetAmbientPreviewDamage() => ModifiedDamage; // getter already recomputes via Hook.ModifyDamage
        public void PreExtraHoverTips() => _ = VengefulDamage;

        public int Duration
        {
            get => DynamicVars[nameof(Duration)].IntValue;
            set
            {
                DynamicVars[nameof(Duration)].BaseValue = value;
                PowerExtensions.InvokeSecondAmountChanged(this);
                if (value == 1) Flash();
            }

        }
        public string GetSecondAmount() => Duration.ToString();
        public Color SecondAmountColor => StsColors.blue;
        public bool ShouldEmphasizeSecondAmount => Duration == 1;

        public override async Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
        {
            if (!participants.Contains(Owner))
            {
                return;
            }

            Duration--;

            if (Duration <= 0)
            {
                await DetonateCmd.Detonate(new ThrowingPlayerChoiceContext(), this);
            }
        }

        // Just in case safety
        private bool _isExploding = false;
        /// <summary>
        /// Reference to the current detonation args.
        /// Gets fed into it from DetonateCMD so that it can provide back the damage result before being removes.
        /// </summary>
        public DetonationEventArgs? currentDetonationArgs;
        public override async Task AfterRemoved(Creature oldOwner)
        {
            if (_isExploding) return;
            _isExploding = true;
            decimal damage = BaseDamage;
            var damageResults = await CreatureCmd.Damage(new ThrowingPlayerChoiceContext(), oldOwner, damage, ValueProp.Move, Applier, null, null);
            var dmgResult = damageResults.FirstOrDefault();
            if (currentDetonationArgs != null)
            {
                currentDetonationArgs.damageResult = dmgResult;
            }
            await base.AfterRemoved(oldOwner);
        }

        public override Task AfterPowerAmountChanged(PlayerChoiceContext choiceContext, PowerModel power, decimal amount, Creature? applier, CardModel? cardSource)
        {
            return base.AfterPowerAmountChanged(choiceContext, power, amount, applier, cardSource);
        }
    }
}
