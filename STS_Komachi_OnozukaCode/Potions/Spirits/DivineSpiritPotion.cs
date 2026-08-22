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
    public class DivineSpiritPotion : STS_Komachi_OnozukaPotion
    {
        public override PotionRarity Rarity => PotionRarity.Uncommon;

        public override PotionUsage Usage => PotionUsage.CombatOnly;

        public override TargetType TargetType => TargetType.AnyPlayer;

        public override IEnumerable<IHoverTip> ExtraHoverTips => [
            HoverTipFactory.FromPower<DivineSpiritPower>(), 
            HoverTipFactory.Static(StaticHoverTip.SummonStatic)
            ];
        protected override IEnumerable<DynamicVar> CanonicalVars => [new PowerVar<DivineSpiritPower>(10)];
        protected override async Task OnUse(PlayerChoiceContext choiceContext, Creature? target)
        {
            AssertValidForTargetedPotion(target);
            await PowerCmd.Apply<DivineSpiritPower>(choiceContext, target, 
                DynamicVars.Power<DivineSpiritPower>().BaseValue, Owner.Creature, null);

        }
    }
}
