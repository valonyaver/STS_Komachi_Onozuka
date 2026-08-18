using BaseLib.Abstracts;
using BaseLib.Extensions;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Cards.Tokens;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Commands;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Extensions;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Powers.Spirits;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Powers.Other
{
    public class GoldSarcophagalleyPower : STS_Komachi_OnozukaPower
    {
        public override PowerType Type => PowerType.Buff;
        public override PowerStackType StackType => PowerStackType.Counter;
        public override PowerInstanceType InstanceType => PowerInstanceType.Instanced;

        protected override IEnumerable<DynamicVar> CanonicalVars =>
        [
            new StringVar("CardName", "???")
        ];

        public string CardName
        {
            get => ((StringVar)DynamicVars["CardName"]).StringValue;
            set => ((StringVar)DynamicVars["CardName"]).StringValue = value;
        }

        protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromKeyword(KomachiKeywords.Replenish)];

        private class TrackedCardData
        {
            public CardModel? Card;
        }

        protected override object? InitInternalData() => new TrackedCardData();
        private TrackedCardData Data => GetInternalData<TrackedCardData>();

        /// <summary>
        /// Called by the card immediately after PowerCmd.Apply — sets the card reference
        /// and name var on the freshly created instance. 
        /// </summary>
        public void TrackCard(CardModel card)
        {
            Data.Card = card;
            CardName = card.Title;
        }

        public override async Task BeforeHandDraw(Player player, PlayerChoiceContext choiceContext, ICombatState combatState)
        {
            if (player.Creature != Owner)
            {
                return;
            }

            SetAmount(Amount-1, true);
            if (Amount <= 0)
            {
                CardModel? card = Data.Card;
                if (card != null)
                {
                    CardCmd.Upgrade(card);
                    CardCmd.ApplyKeyword(card, KomachiKeywords.Replenish);
                    await CardPileCmd.Add(card, PileType.Hand);
                }

                await PowerCmd.Remove(this);
            }
        }
    }
}
