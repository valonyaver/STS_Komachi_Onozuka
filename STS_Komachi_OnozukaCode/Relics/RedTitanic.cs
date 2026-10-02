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
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.ValueProps;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Cards.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Relics
{
    public class RedTitanic : STS_Komachi_OnozukaRelic
    {
        public override RelicRarity Rarity => RelicRarity.Starter;
        protected override IEnumerable<IHoverTip> ExtraHoverTips =>
            [
                HoverTipFactory.FromCard<SpiderLily>(),
                HoverTipFactory.FromCard<ManipulateDistanceToken>()
            ];

        protected override IEnumerable<DynamicVar> CanonicalVars => 
            [
            new HealVar(0)
            ];

        public override bool ShowCounter => true;
        public override int DisplayAmount => DynamicVars.Heal.IntValue;
        public override async Task BeforeHandDraw(Player player, PlayerChoiceContext choiceContext, ICombatState combatState)
        {
            if (player != Owner)
                return;
            if (player.PlayerCombatState?.TurnNumber == 1)
            {
                DynamicVars.Heal.BaseValue = 0;
                InvokeDisplayAmountChanged();
                List<CardModel> list1 = [];
                CardModel mandist = combatState.CreateCard<ManipulateDistanceToken>(Owner);
                list1.Add(mandist);
                CardModel lily1 = combatState.CreateCard<SpiderLily>(Owner);
                list1.Add(lily1);
                await CardPileCmd.AddGeneratedCardsToCombat(list1, PileType.Hand, Owner);


                List<CardModel> list2 = [];
                for (int i = 0; i < 3; i++)
                {
                    CardModel lily2 = combatState.CreateCard<SpiderLily>(Owner);
                    list2.Add(lily2);
                }

                CardCmd.PreviewCardPileAdd(await CardPileCmd.AddGeneratedCardsToCombat(list2, PileType.Discard, Owner, CardPilePosition.Random));

                Flash();
            }
        }

        public override async Task AfterDamageReceived(PlayerChoiceContext choiceContext, Creature target, DamageResult result, ValueProp props, Creature? dealer, CardModel? cardSource)
        {
            if (target == Owner.Creature &&
                // Same props poison uses
                (props.HasFlag(ValueProp.Unblockable | ValueProp.Unpowered))
                && Owner.PlayerCombatState != null) 
            {
                DynamicVars.Heal.BaseValue += result.UnblockedDamage;
                InvokeDisplayAmountChanged();
            }
        }

        public override async Task AfterCombatVictory(CombatRoom room)
        {
            if (!base.Owner.Creature.IsDead)
            {
                Flash();
                await CreatureCmd.Heal(base.Owner.Creature, base.DynamicVars.Heal.BaseValue);
                DynamicVars.Heal.BaseValue = 0;
                InvokeDisplayAmountChanged();
            }
        }
    }
}
