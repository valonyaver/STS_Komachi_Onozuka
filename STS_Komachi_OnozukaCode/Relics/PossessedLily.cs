using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Rooms;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Cards.Tokens;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Powers.Spirits;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Relics
{
    public class PossessedLily : STS_Komachi_OnozukaRelic
    {
        public override RelicRarity Rarity => RelicRarity.Rare;

        protected override IEnumerable<DynamicVar> CanonicalVars => [
            new PowerVar<DivineSpiritPower>(1),
            ];
        protected override IEnumerable<IHoverTip> ExtraHoverTips =>
            [
                HoverTipFactory.FromPower<PoisonPower>(),
                HoverTipFactory.FromPower<DivineSpiritPower>(),
            ];

        public override async Task AfterPowerAmountChanged(PlayerChoiceContext choiceContext, PowerModel power, decimal amount, Creature? applier, CardModel? cardSource)
        {
            if (power is PoisonPower && power.Owner == Owner.Creature && amount > 0)
            {
                Flash();
                await PowerCmd.Apply<DivineSpiritPower>(choiceContext, Owner.Creature, amount, Owner.Creature, null);
            }
        }
    }
}
