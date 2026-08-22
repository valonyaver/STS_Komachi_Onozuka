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
using MegaCrit.Sts2.Core.Models.Events;
using MegaCrit.Sts2.Core.Models.Relics;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Cards.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Relics
{
    public class TheTitanic : STS_Komachi_OnozukaRelic
    {
        public override RelicRarity Rarity => RelicRarity.Starter;
        protected override IEnumerable<IHoverTip> ExtraHoverTips =>
            [
                HoverTipFactory.FromCard<SpiderLily>(),
                HoverTipFactory.FromCard<ManipulateDistanceToken>(),
            ];

        public override async Task BeforeHandDraw(Player player, PlayerChoiceContext choiceContext, ICombatState combatState)
        {
            if (player != Owner)
                return;
            if (player.PlayerCombatState?.TurnNumber == 1)
            {
                Flash();
                CardModel created = combatState.CreateCard<ManipulateDistanceToken>(Owner);
                await CardPileCmd.AddGeneratedCardToCombat(created, PileType.Hand, Owner);
            }
            else if (player.PlayerCombatState?.TurnNumber == 3)
            {
                Flash();
                CardModel created = combatState.CreateCard<SpiderLily>(Owner);
                await CardPileCmd.AddGeneratedCardToCombat(created, PileType.Hand, Owner);
            }
        }

        // Old effect of adding on enemy death.
        //public override async Task AfterDeath(PlayerChoiceContext choiceContext, Creature creature, bool wasRemovalPrevented, float deathAnimLength)
        //{
        //    if (creature.Side != base.Owner.Creature.Side)
        //    {
        //        Flash();
        //        CardModel created = Owner.Creature.CombatState.CreateCard<SpiderLily>(Owner);
        //        await CardPileCmd.Add(created, PileType.Hand);
        //    }
        //}

        public override RelicModel? GetUpgradeReplacement()
        {
            if (!IsMutable) return ModelDb.Relic<CommercialTitanic>();
            RelicModel[] list = [ModelDb.Relic<CommercialTitanic>(), ModelDb.Relic<RedTitanic>()];
            return Owner.RunState.Rng.Niche.NextItem(list);
        }
    }
}
