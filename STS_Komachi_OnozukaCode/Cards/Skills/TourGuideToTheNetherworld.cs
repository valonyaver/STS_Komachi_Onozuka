using BaseLib.Extensions;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.ValueProps;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Cards.Tokens;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Commands;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Extensions;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Powers.Distance;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Powers.Spirits;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Cards.Attack
{
      
    public class TourGuideToTheNetherworld : STS_Komachi_OnozukaCard
    {
        public TourGuideToTheNetherworld()
        : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
        {
            // Draw amount
            WithCards(2, 1);
            WithPower<VengefulSpiritPower>(nameof(Value1), 3);
            WithPower<GuidedSpiritPower>(nameof(Value2), 2);
            WithPower<DivineSpiritPower>(nameof(Value3), 2);
            WithKeyword(KomachiKeywords.Barrier);
        }

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            var draws = await CardPileCmd.Draw(choiceContext, DynamicVars.Cards.BaseValue, Owner);

            foreach(var card in draws)
            {
                switch(card.Rarity)
                {
                    case CardRarity.Uncommon:
                        await PowerCmd.Apply<GuidedSpiritPower>(choiceContext, Owner.Creature, Value2, Owner.Creature, this);
                        break;
                    case CardRarity.Rare:
                    case CardRarity.Ancient:
                        await PowerCmd.Apply<DivineSpiritPower>(choiceContext, Owner.Creature, Value3, Owner.Creature, this);
                        break;
                    default:
                        await PowerCmd.Apply<VengefulSpiritPower>(choiceContext, CombatState.HittableEnemies, Value1, Owner.Creature, this);
                        break;
                }
            }
        }
    }
}
