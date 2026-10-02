using BaseLib.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Potions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Potions;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Cards.Tokens;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Powers.Spirits;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Potions
{
    public class VengefulSpiritPotion : STS_Komachi_OnozukaPotion
    {
        public override PotionRarity Rarity => PotionRarity.Common;

        public override PotionUsage Usage => PotionUsage.CombatOnly;

        public override TargetType TargetType => TargetType.AnyEnemy;

        public override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<VengefulSpiritPower>()];
        protected override IEnumerable<DynamicVar> CanonicalVars => [new PowerVar<VengefulSpiritPower>(20)];
        protected override async Task OnUse(PlayerChoiceContext choiceContext, Creature? target)
        {
            AssertValidForTargetedPotion(target);
            await PowerCmd.Apply<VengefulSpiritPower>(choiceContext, target, 
                DynamicVars.Power<VengefulSpiritPower>().BaseValue, Owner.Creature, null);
        }
    }
}
