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
    public class ShinigamiTactics : STS_Komachi_OnozukaCard
    {
        public ShinigamiTactics()
            : base(2, CardType.Power, CardRarity.Rare, TargetType.Self)
        {
            WithTip(typeof(ManipulateDistanceToken));
            WithTip(typeof(DistancePower));
            WithTip(KomachiKeywords.Replenish);
            WithKeyword(KomachiKeywords.Displace);

        }

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            if (CombatState == null) return;

            if (IsUpgraded)
            {
                var card = CombatState.CreateCard<ManipulateDistanceToken>(Owner);

                await CardPileCmd.AddGeneratedCardToCombat(card, PileType.Hand, base.Owner);
            }

            await PowerCmd.Apply<ShinigamiTacticsPower>(choiceContext, Owner.Creature,
                1, Owner.Creature, this);

        }
    }
}
