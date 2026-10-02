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
using MegaCrit.Sts2.Core.ValueProps;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Cards.Tokens;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Extras;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Powers.Distance;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Relics
{
    public class ParryingScythe : STS_Komachi_OnozukaRelic
    {

        public bool _usedThisCombat;
        public bool UsedThisCombat
        {
            get
            {
                return _usedThisCombat;
            }
            set
            {
                if (_usedThisCombat != value)
                {
                    AssertMutable();
                    _usedThisCombat = value;
                }
            }
        }
        public override RelicRarity Rarity => RelicRarity.Rare;
        protected override IEnumerable<IHoverTip> ExtraHoverTips =>
            [
                HoverTipFactory.FromPower<DistancePower>(),
            ];

        public override async Task BeforeDamageReceived(PlayerChoiceContext choiceContext, Creature target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
        {
            // Ensure the relic is ready, the dealer exists, it's an enemy attack, and distance < 3
            if (!UsedThisCombat
                && dealer != null
                && dealer.Side == CombatSide.Enemy
                && props.IsPoweredAttack()
                && amount > 0
                && target == Owner.Creature
                && DistancePower.GetLevel(dealer) < 3)
            {
                Flash();
                await CreatureCmd.GainBlock(Owner.Creature, amount / 2m, ValueProp.Unpowered, null);
                UsedThisCombat = true;
            }
        }

        public override Task AfterCombatEnd(CombatRoom _)
        {
            UsedThisCombat = false;
            return Task.CompletedTask;
        }
    }
}
