using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Cards.Tokens;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Danmaku;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Cards.Attack
{
    public class SidestepStrike : STS_Komachi_OnozukaCard
    {
        public SidestepStrike()
            : base(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
        {
            WithDamage(8);
            WithTags(CardTag.Strike);
            WithTip(typeof(ManipulateDistanceToken));
        }

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            if (CombatState == null) return;

            await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue).FromCard(this, cardPlay).Targeting(cardPlay.Target)
            .WithHitFx("vfx/vfx_attack_slash", null)
            .Execute(choiceContext);

            bool hasTokenInHand = Owner.PlayerCombatState?.Hand.Cards.OfType<ManipulateDistanceToken>().Any() ?? false;

            if (IsUpgraded || !hasTokenInHand)
            {
                var mandist = CombatState.CreateCard<ManipulateDistanceToken>(Owner);
                await CardPileCmd.AddGeneratedCardToCombat(mandist, PileType.Hand, Owner);
            }
        }

        public override List<DanmakuPiece> patterns => [

            ];
    }
}
