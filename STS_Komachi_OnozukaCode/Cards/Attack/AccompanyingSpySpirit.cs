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
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Powers.Distance;

namespace STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Cards.Attack
{
      
    public class AccompanyingSpySpirit : STS_Komachi_OnozukaCard
    {
        public AccompanyingSpySpirit()
        : base(2, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
        {
            WithDamage(13, 3);
            WithKeyword(KomachiKeywords.Displace);
            WithKeyword(KomachiKeywords.Barrier);
            WithKeyword(CardKeyword.Exhaust);
            WithTip(typeof(DistancePower));
            // Summon from displacement
            WithVar(nameof(Value1), 1, 1);
        }
        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");
            await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue).FromCard(this, cardPlay).Targeting(cardPlay.Target)
                .WithHitFx("vfx/vfx_attack_slash")
                .Execute(choiceContext);
            await PowerCmd.Apply<AccompanyingSpySpiritPower>(
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
