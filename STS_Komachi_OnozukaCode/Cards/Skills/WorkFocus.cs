using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Potions;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using MegaCrit.Sts2.GameInfo.Objects;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Cards.Attack;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Cards.Tokens;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Commands;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Extensions;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Powers.Abilities;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Powers.Spirits;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static BaseLib.Utils.BetaMainCompatibility;
using static Godot.OpenXRCompositionLayer;

namespace STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Cards
{
      
    public class WorkFocus : STS_Komachi_OnozukaCard
    {
        public WorkFocus() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
        {
            WithKeyword(CardKeyword.Exhaust);
            WithVar(nameof(Value1), 2);
            WithTip(typeof(ManipulateDistanceToken));
            WithTip(typeof(SpiderLily));
        }

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            if (Owner.PlayerCombatState == null) return;
            List<CardModel> list = [.. Owner.PlayerCombatState.Hand.Cards.Where(c => c != this)];
            List<CardModel> choice;
            if (IsUpgraded)
            {
                List<CardModel> draw = PileType.Draw.GetPile(Owner).Cards.ToList();
                // Needs to be resorted so as to not show the actual order of cards
                draw = (from c in draw
                            orderby c.Rarity, c.Id
                            select c).ToList();
                
                list.AddRange(draw);
                // list.AddRange(Owner.PlayerCombatState.DiscardPile.Cards); 
                choice = (await CardSelectCmd.FromSimpleGrid(
                        choiceContext,
                        list,
                        Owner,
                        new CardSelectorPrefs(SelectionScreenPrompt, 1, Value1))).ToList();

            }
            else
            {
                choice = (await CardSelectCmd.FromHand(
                    prefs: new CardSelectorPrefs(CardSelectorPrefs.ExhaustSelectionPrompt, 1, Value1), 
                    context: choiceContext, 
                    player: base.Owner, 
                    filter: null, 
                    source: this)).ToList();

            }


            if (choice.Count > 0)
            {
                foreach(var card in  choice)
                {
                    await CardCmd.Exhaust(choiceContext, card);
                }

                await ManipulateDistanceToken.CreateInHand(Owner, choice.Count, CombatState);
                if (choice.Any(c => c.IsUpgraded)) {
                    var lily = CombatState.CreateCard<SpiderLily>(Owner);
                    await CardPileCmd.AddGeneratedCardToCombat(lily, PileType.Hand, Owner);
                }
            }
        }
    }
}
