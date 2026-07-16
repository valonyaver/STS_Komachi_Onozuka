using BaseLib.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Powers.Spirits;
using STS_Komachi_Onozuka;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Extensions;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Powers.Abilities;

namespace STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Cards.Attack
{
      
    public class LonelyBoundSpirit : STS_Komachi_OnozukaCard
    {
        public LonelyBoundSpirit()
        : base(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
        {
            WithDamage(8, 3);
            WithPower<VengefulSpiritPower>(3, 1);
            WithPower<LonelyBoundSpiritPower>(nameof(Value1), 1);
            WithKeyword(KomachiKeywords.Detonate);
        }
        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");
            await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue).FromCard(this, cardPlay).Targeting(cardPlay.Target)
                .WithHitFx("vfx/vfx_attack_slash")
                .Execute(choiceContext);
            await PowerCmd.Apply<LonelyBoundSpiritPower>(
                choiceContext, cardPlay.Target, 
                Value1, Owner.Creature, this);
        }

        //protected override void OnUpgrade()
        //{
        //    base.DynamicVars.Damage.UpgradeValueBy(2);
        //    DynamicVars[nameof(VengefulSpiritApplication)].UpgradeValueBy(1);
        //}
    }
}
