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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Relics
{
    public class ShinigamiEyes : STS_Komachi_OnozukaRelic
    {
        public override RelicRarity Rarity => RelicRarity.Common;

        protected override IEnumerable<DynamicVar> CanonicalVars => [
            new PowerVar<DexterityPower>(1),
            new PowerVar<VulnerablePower>(2), // 2 because the enemy will lose one upon their turn immediately
            ];
        protected override IEnumerable<IHoverTip> ExtraHoverTips =>
            [
                HoverTipFactory.FromPower<DexterityPower>(),
                HoverTipFactory.FromPower<VulnerablePower>(),
            ];

        public override async Task AfterRoomEntered(AbstractRoom room)
        {
            if (room is not CombatRoom)
                return;
            Flash();
            await PowerCmd.Apply<DexterityPower>(new ThrowingPlayerChoiceContext(), Owner.Creature, DynamicVars["DexterityPower"].BaseValue, Owner.Creature, null);
        
        }

        public override async Task BeforeSideTurnEndVeryEarly(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
        {
            if (!participants.Contains(Owner.Creature) || Owner.PlayerCombatState.TurnNumber != 3)
                return;
            await PowerCmd.Apply<DexterityPower>(choiceContext, Owner.Creature, -DynamicVars["DexterityPower"].BaseValue, Owner.Creature, null);
            await PowerCmd.Apply<VulnerablePower>(choiceContext, Owner.Creature.CombatState.HittableEnemies, DynamicVars["VulnerablePower"].BaseValue, Owner.Creature, null);

        }
    }
}
