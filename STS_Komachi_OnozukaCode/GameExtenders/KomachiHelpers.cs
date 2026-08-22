using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Extras
{
    internal static class KomachiHelpers
    {
        /// <summary>
        /// Finds the damage that would be dealt from a creature to a target creature and updates the given var.
        /// </summary>
        public static decimal FindDamageDealt(Creature? dealer, Creature? target, decimal baseDamage, DynamicVar damageVar)
        {
            decimal modified = baseDamage;
            if (target != null && dealer?.CombatState != null && dealer.Player?.RunState != null)
            {
                modified = Hook.ModifyDamage(
                    dealer.Player.RunState, dealer.CombatState, target, dealer,
                    baseDamage, ValueProp.Move, null,null, ModifyDamageHookType.All, CardPreviewMode.None, out _);
            }
            damageVar.BaseValue = baseDamage;
            damageVar.PreviewValue = modified;
            return modified;
        }
    }
}
