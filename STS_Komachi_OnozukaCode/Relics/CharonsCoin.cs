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
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Runs;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Cards.Tokens;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Powers.Spirits;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Relics
{
    public class CharonsCoin : STS_Komachi_OnozukaRelic
    {
        public override bool IsAllowedInShops => false;

        public override bool HasUponPickupEffect => true;

        public override RelicRarity Rarity => RelicRarity.Uncommon;
        protected override IEnumerable<IHoverTip> ExtraHoverTips =>
            [
                HoverTipFactory.FromPower<GuidedSpiritPower>(),
            ];

        protected override IEnumerable<DynamicVar> CanonicalVars => [
            new GoldVar("GoldGain",44),
            new GoldVar(4),
            new PowerVar<GuidedSpiritPower>(4)
            ];


        public override bool IsAllowed(IRunState runState)
        {
            return RelicModel.IsBeforeAct3TreasureChest(runState);
        }

        public override async Task AfterObtained()
        {
            await PlayerCmd.GainGold(base.DynamicVars["GoldGain"].BaseValue, base.Owner);
        }

        /// <summary>
        /// Gain Gold on kill
        /// </summary>
        public override async Task AfterDeath(PlayerChoiceContext choiceContext, Creature creature, bool wasRemovalPrevented, float deathAnimLength)
        {
            if (creature.Side != base.Owner.Creature.Side)
            {
                await PowerCmd.Apply<GuidedSpiritPower>(choiceContext,
                    Owner.Creature, DynamicVars["GuidedSpiritPower"].BaseValue, Owner.Creature, null);
                if (!creature.IsSecondaryEnemy)
                {
                    await PlayerCmd.GainGold(DynamicVars.Gold.BaseValue, Owner);
                }
            }
        }
    }
}
