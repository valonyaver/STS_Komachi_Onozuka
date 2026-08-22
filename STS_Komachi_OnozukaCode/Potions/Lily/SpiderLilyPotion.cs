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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Potions
{
    public class SpiderLilyPotion : STS_Komachi_OnozukaPotion
    {
        public override PotionRarity Rarity => PotionRarity.Uncommon;

        public override PotionUsage Usage => PotionUsage.CombatOnly;

        public override TargetType TargetType => TargetType.Self;

        public override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromCard<SpiderLily>()];
        protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(2)];
        protected override async Task OnUse(PlayerChoiceContext choiceContext, Creature? target)
        {
            for(int i = 0; i < 2; i++)
            {
                CardModel lily = Owner.Creature.CombatState.CreateCard<SpiderLily>(Owner);
                CardPileCmd.AddGeneratedCardToCombat(lily, PileType.Hand, Owner);
            }
        }
    }
}
