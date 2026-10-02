using BaseLib.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.ValueProps;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Cards.Tokens;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Commands;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Danmaku;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Extensions;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Powers.Distance;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Powers.Spirits;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Cards.Attack
{
      
    public class TasteOfDeath : STS_Komachi_OnozukaCard
    {
        public TasteOfDeath()
        : base(2, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
        {
            WithDamage(13, 4);
            WithPower<DistancePower>(nameof(Value1), -5);
            WithKeyword(KomachiKeywords.Displace);
        }

        /// <summary>
        /// Cancels whatever multiplier DistancePower is about to apply to this card's own hit,
        /// then re-applies a flat x2 on top. Both this hook and DistancePower's hook read the
        /// target's live state at the same instant damage is calculated, so the cancellation
        /// holds whether that instant is the hover-preview (pre-displacement) or the actual
        /// dealt damage (post-displacement, since OnPlay displaces before dealing damage) —
        /// the result is always base * 2 either way.
        /// </summary>
        public override decimal ModifyDamageMultiplicative(Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource, CardPlay? cardPlay)
        {
            if (cardSource != this || target == null)
            {
                return 1m;
            }

            decimal m = DistancePower.GetDamageMultiplier(DistancePower.GetLevel(target));
            return 2m / m;
        }

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");

            await DistanceCmd.Displace(choiceContext, cardPlay.Target, Value1, Owner.Creature, this);

            await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this, cardPlay).Targeting(cardPlay.Target)
                .WithHitFx("vfx/vfx_attack_slash")
                .Execute(choiceContext);
        }

        public override List<DanmakuPiece> patterns => [

            ];
    }
}
