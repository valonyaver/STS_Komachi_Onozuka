using BaseLib.Utils;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Cards;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Commands;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Extensions;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Powers.Distance;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Powers.Spirits;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Cards.Tokens
{
    [Pool(typeof(TokenCardPool))]
    public class ManipulateDistanceToken : STS_Komachi_OnozukaCard
    {

        public override bool CanBeGeneratedInCombat => false;
        public ManipulateDistanceToken() : base(0, CardType.Skill, CardRarity.Token, TargetType.AnyEnemy)
        {
            WithKeywords(CardKeyword.Retain, CardKeyword.Exhaust, KomachiKeywords.Displace);
            WithVar(nameof(Value1), 1, 1);
            WithTip(typeof(DistancePower));
        }

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            int[] options = IsUpgraded ? new[] { -2, -1, 1, 2 } : new[] { -1, 1 };
            await DistanceCmd.ChooseAndDisplace(choiceContext, cardPlay.Target, this, options);
        }

        public static async Task<CardModel?> CreateInHand(Player owner, ICombatState combatState)
        {
            return (await CreateInHand(owner, 1, combatState)).FirstOrDefault();
        }

        public static async Task<IEnumerable<CardModel>> CreateInHand(Player owner, int count, ICombatState combatState)
        {
            if (count == 0)
            {
                return [];
            }

            if (CombatManager.Instance.IsOverOrEnding)
            {
                return [];
            }

            List<CardModel> mandists = new List<CardModel>();
            for (int i = 0; i < count; i++)
            {
                mandists.Add(combatState.CreateCard<ManipulateDistanceToken>(owner));
            }

            await CardPileCmd.AddGeneratedCardsToCombat(mandists, PileType.Hand, owner);
            return mandists;
        }
    }
}
