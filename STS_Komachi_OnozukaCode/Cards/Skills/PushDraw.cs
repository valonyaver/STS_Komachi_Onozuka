using BaseLib.Extensions;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.ValueProps;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Cards.Tokens;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Commands;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Extensions;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Powers.Distance;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Powers.Spirits;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Cards.Attack
{
      
    public class PushDraw : STS_Komachi_OnozukaCard
    {
        public PushDraw()
        : base(1, CardType.Skill, CardRarity.Common, TargetType.AnyEnemy)
        {
            WithPower<DistancePower>(nameof(Value1), 1, 1);
            WithKeyword(KomachiKeywords.Displace);
        }

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");
            //// Displacement happens before block for certain powers that care about it.
            //var enemyDisplacement = DistancePower.GetLevel(cardPlay.Target);

            //if (enemyDisplacement < 5)
            //{
            //    int[] options = IsUpgraded ? new[] { 0, 1, 2 } : new[] { 0, 1 };
            //    await DistanceCmd.ChooseAndDisplace(choiceContext, cardPlay.Target, this, options);
            //}
            if (IsUpgraded)
            {
                int[] options = [1, 2];
                await DistanceCmd.ChooseAndDisplace(choiceContext, cardPlay.Target, this, options);
            }
            else await DistanceCmd.Displace(choiceContext, cardPlay.Target, 1, Owner.Creature, this);

            var enemyDisplacement = DistancePower.GetLevel(cardPlay.Target);


            await CardPileCmd.Draw(choiceContext, enemyDisplacement, Owner);

            CardModel cardModel = (await CardSelectCmd.FromHandForDiscard(choiceContext, base.Owner, new CardSelectorPrefs(CardSelectorPrefs.DiscardSelectionPrompt, 1), null, this)).FirstOrDefault();
            if (cardModel != null)
            {
                await CardCmd.Discard(choiceContext, cardModel);
            }
        }
    }
}
