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
    public class DeathNote : STS_Komachi_OnozukaRelic
    {
        public override RelicRarity Rarity => RelicRarity.Rare;

        protected override IEnumerable<DynamicVar> CanonicalVars => [
            new PowerVar<VengefulSpiritPower>(4),
            ];
        protected override IEnumerable<IHoverTip> ExtraHoverTips =>
            [
                HoverTipFactory.FromPower<VengefulSpiritPower>(),
            ];

        public override async Task AfterCardExhausted(PlayerChoiceContext choiceContext, CardModel card, bool _)
        {
            if (card.Owner == base.Owner)
            {
                Flash();
                await PowerCmd.Apply<VengefulSpiritPower>(choiceContext, Owner.Creature.CombatState.HittableEnemies, DynamicVars["VengefulSpiritPower"].BaseValue, Owner.Creature, null);
            }
        }
    }
}
