using BaseLib.Extensions;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Enchantments;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.ValueProps;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Cards.Tokens;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Commands;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Extensions;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Powers.Distance;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Powers.Other;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Powers.Spirits;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Cards.Attack
{
      
    public class GoldSarcophagalley : STS_Komachi_OnozukaCard
    {
        public GoldSarcophagalley()
        : base(0, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
        {
            // draw
            WithVar(nameof(Value1), 2);
            WithKeyword(CardKeyword.Innate, UpgradeType.Add);
            WithKeyword(CardKeyword.Exhaust);
            WithKeyword(KomachiKeywords.Replenish);
        }

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            await CardPileCmd.ShuffleIfNecessary(choiceContext, Owner);

            CardPile drawPile = PileType.Draw.GetPile(Owner);

            CardModel? selected = (await CardSelectCmd.FromCombatPile(
                choiceContext,
                drawPile,
                Owner,
                new CardSelectorPrefs(SelectionScreenPrompt, 1)
            )).FirstOrDefault();

            if (selected == null) return;

            await CardCmd.Exhaust(choiceContext, selected);

            var appliedPower = await PowerCmd.Apply<GoldSarcophagalleyPower>(choiceContext, Owner.Creature, 2, Owner.Creature, this);

            appliedPower?.TrackCard(selected);
        }
    }
}
