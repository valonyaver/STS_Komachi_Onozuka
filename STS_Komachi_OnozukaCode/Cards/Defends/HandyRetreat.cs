using BaseLib.Extensions;
using BaseLib.Utils;
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
      
    public class HandyRetreat : STS_Komachi_OnozukaCard
    {
        public HandyRetreat()
        : base(1, CardType.Skill, CardRarity.Basic, TargetType.AnyEnemy)
        {
            WithBlock(7, 2);
            WithPower<DistancePower>(nameof(Value1), 1, 1);
            WithKeyword(KomachiKeywords.Displace);

            WithTip(typeof(ManipulateDistanceToken));
        }

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");
            // Displacement happens before block for certain powers that care about it.
            var enemyDisplacement = DistancePower.GetLevel(cardPlay.Target);

            if (enemyDisplacement < 5)
            {
                if (!IsUpgraded)
                {
                    await DistanceCmd.Displace(choiceContext, cardPlay.Target, 1, Owner.Creature, this);
                }
                else await DistanceCmd.ChooseAndDisplace(choiceContext, cardPlay.Target, this, [1, 2]);
            }

            await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);

            enemyDisplacement = DistancePower.GetLevel(cardPlay.Target);
            if (enemyDisplacement >= 5)
            {
                var mandist = CombatState.CreateCard<ManipulateDistanceToken>(Owner);
                await CardPileCmd.AddGeneratedCardToCombat(mandist, PileType.Hand, Owner);
            }
        }
    }
}
