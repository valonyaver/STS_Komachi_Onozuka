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
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Cards.Tokens;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Commands;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Powers.Spirits;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Relics
{
    public class CommercialTitanic : STS_Komachi_OnozukaRelic, IOnDistanceChangedListener
    {
        public override RelicRarity Rarity => RelicRarity.Starter;
        protected override IEnumerable<IHoverTip> ExtraHoverTips =>
            [
                HoverTipFactory.FromCard<SpiderLily>(),
                HoverTipFactory.FromCard<ManipulateDistanceToken>(),
                HoverTipFactory.FromPower<GuidedSpiritPower>(),
            ];

        protected override IEnumerable<DynamicVar> CanonicalVars => [
            new PowerVar<GuidedSpiritPower>(1)
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

        public async Task OnDistanceChanged(PlayerChoiceContext choiceContext, DistanceChangedEventArgs args)
        {
            if (args.Applier == Owner.Creature && args.ChangeAbs > 0)
            {
                Flash();
                await PowerCmd.Apply<GuidedSpiritPower>(choiceContext,
                    Owner.Creature, DynamicVars["GuidedSpiritPower"].BaseValue, Owner.Creature, null);
            }
        }
    }
}
