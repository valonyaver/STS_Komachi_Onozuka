using BaseLib.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Danmaku;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Powers.Spirits;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Cards.Attack
{
      
    public class LowTierShinigami : STS_Komachi_OnozukaCard
    {
        public LowTierShinigami()
        : base(2, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy)
        {
            WithDamage(18);
            WithPower<VulnerablePower>(nameof(Value1), 99);
            WithKeyword(CardKeyword.Exhaust);
            WithKeyword(CardKeyword.Innate, UpgradeType.Add);
        }
        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");
            await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue).FromCard(this, cardPlay).Targeting(cardPlay.Target)
                .WithHitFx("vfx/vfx_attack_slash")
                .Execute(choiceContext);
            await PowerCmd.Apply<VulnerablePower>(
                choiceContext, cardPlay.Target, 
                Value1, Owner.Creature, this);
        }


        public override List<DanmakuPiece> patterns => [

            ];
    }
}
