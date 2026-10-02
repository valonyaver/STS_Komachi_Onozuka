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
      
    public class EqualizeDistance : STS_Komachi_OnozukaCard
    {
        public EqualizeDistance()
        : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.AnyEnemy)
        {
            WithTip(typeof(DistancePower));
            WithKeyword(KomachiKeywords.Displace);
            WithKeyword(CardKeyword.Retain, UpgradeType.Add);
            // Draw per displacement
            WithVar(nameof(Value1), 2);
            // Draw on 3 distance
            WithVar(nameof(Value2), 3);
            WithTip(typeof(ManipulateDistanceToken));
        }

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");
            // Displacement happens before block for certain powers that care about it.
            var enemyDisplacement = DistancePower.GetLevel(cardPlay.Target);

            var deltaChange = 3 - enemyDisplacement;

            int DrawAmount;
            if (deltaChange != 0)
            {
                var displace = await DistanceCmd.Displace(choiceContext, cardPlay.Target, deltaChange, Owner.Creature);
                DrawAmount = displace.ChangeAbs * Value1;
            }
            else
            {
                DrawAmount = Value2;
            }



            await CardPileCmd.Draw(choiceContext, DrawAmount, Owner);
            // If 3, discard 1 after drawing
            if (deltaChange == 0)
            {
                //CardModel cardModel = (await CardSelectCmd.FromHandForDiscard(choiceContext, base.Owner, new CardSelectorPrefs(CardSelectorPrefs.DiscardSelectionPrompt, 1), null, this)).FirstOrDefault();
                //if (cardModel != null)
                //{
                //    await CardCmd.Discard(choiceContext, cardModel);
                //}
                await ManipulateDistanceToken.CreateInHand(Owner, CombatState);
            }
        }
    }
}
