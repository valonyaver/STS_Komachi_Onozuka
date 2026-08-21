using BaseLib.Abstracts;
using BaseLib.Extensions;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Factories;
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

namespace STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Powers.Abilities
{ 
    public class ShinigamiTacticsPower : STS_Komachi_OnozukaPower, IOnDistanceChangedListener
    {
        public override PowerType Type => PowerType.Buff;
        public override PowerStackType StackType => PowerStackType.Counter;

        public async Task OnDistanceChanged(PlayerChoiceContext choiceContext, DistanceChangedEventArgs args)
        {
            if (Owner.Player == null) return;
            if (!(args.cardSource is ManipulateDistanceToken)) return;

            List<CardModel> cardsToBeAdded = new();
            // Generate a skill card
            if (args.Change > 0)
            {
                IEnumerable<CardModel> cards =
                    from c in base.Owner.Player.Character.CardPool.GetUnlockedCards(base.Owner.Player.UnlockState, base.Owner.Player.RunState.CardMultiplayerConstraint)
                    where c.Type == CardType.Skill
                    select c;
                cardsToBeAdded = CardFactory.GetDistinctForCombat(base.Owner.Player, cards, Amount, base.Owner.Player.RunState.Rng.CombatCardGeneration).ToList();

            }
            // Generate an attack
            else if (args.Change < 0)
            {
                IEnumerable<CardModel> cards =
                    from c in base.Owner.Player.Character.CardPool.GetUnlockedCards(base.Owner.Player.UnlockState, base.Owner.Player.RunState.CardMultiplayerConstraint)
                    where c.Type == CardType.Attack
                    select c;
                cardsToBeAdded = CardFactory.GetDistinctForCombat(base.Owner.Player, cards, Amount, base.Owner.Player.RunState.Rng.CombatCardGeneration).ToList();
            }

            if (cardsToBeAdded == null || cardsToBeAdded.Count <= 0)
            {
                return;
            }
            foreach(var card in cardsToBeAdded)
            {
                card.AddKeyword(KomachiKeywords.Replenish);
            }
            await CardPileCmd.AddGeneratedCardsToCombat(cardsToBeAdded, PileType.Hand, Owner.Player);
        }
    }
}
