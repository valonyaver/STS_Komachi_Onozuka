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
    public class SpiritCompanion : STS_Komachi_OnozukaRelic
    {
        public override RelicRarity Rarity => RelicRarity.Common;

        protected override IEnumerable<DynamicVar> CanonicalVars => [
            new PowerVar<GuidedSpiritPower>(5),
            ];
        protected override IEnumerable<IHoverTip> ExtraHoverTips =>
            [
                HoverTipFactory.FromPower<GuidedSpiritPower>(),
            ];

        public override async Task AfterRoomEntered(AbstractRoom room)
        {
            if (room is not CombatRoom)
                return;
            Flash();
            await PowerCmd.Apply<GuidedSpiritPower>(new ThrowingPlayerChoiceContext(), Owner.Creature, DynamicVars["GuidedSpiritPower"].BaseValue, Owner.Creature, null);
        }
    }
}
