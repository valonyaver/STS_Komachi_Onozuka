using BaseLib.Utils;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Cards.Tokens;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Commands;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Extensions;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Powers.Abilities;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Powers.Distance;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Powers.Spirits;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Cards
{
    public class RiversideGarden : STS_Komachi_OnozukaCard
    {

        public RiversideGarden()
            : base(3, CardType.Power, CardRarity.Rare, TargetType.Self)
        {
            WithTip(new TooltipSource((c) =>
                HoverTipFactory.FromCard<SpiderLily>(true))
                );
            WithKeyword(CardKeyword.Innate, UpgradeType.Add);
        }

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            if (CombatState == null) return;

            await PowerCmd.Apply<RiversideGardenPower>(
                choiceContext, Owner.Creature, 1, Owner.Creature, this);

            SpiderLily lily1 = CombatState.CreateCard<SpiderLily>(Owner);
            await CardPileCmd.AddGeneratedCardToCombat(lily1, PileType.Hand, Owner);

            SpiderLily lily2 = CombatState.CreateCard<SpiderLily>(Owner);
            CardCmd.PreviewCardPileAdd(await CardPileCmd.AddGeneratedCardToCombat(lily2, PileType.Draw, Owner, CardPilePosition.Random));

            SpiderLily lily3 = CombatState.CreateCard<SpiderLily>(Owner);
            CardCmd.PreviewCardPileAdd(await CardPileCmd.AddGeneratedCardToCombat(lily3, PileType.Discard, Owner));


        }
    }
}
