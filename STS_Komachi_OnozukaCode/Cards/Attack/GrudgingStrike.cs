using BaseLib.Extensions;
using BaseLib.Utils;
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
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Powers.Spirits;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Cards.Attack
{
      
    public class GrudgingStrike : STS_Komachi_OnozukaCard
    {
        public GrudgingStrike()
        : base(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
        {
            WithDamage(8, 2);
            WithPower<VengefulSpiritPower>(nameof(VengefulSpiritApplication), 3, 1);
            WithVar(nameof(ReleaseCost), 3, -1);
            WithTags(CardTag.Strike);
            WithTip( new TooltipSource( (c)=>
                HoverTipFactory.FromCard<DetonateToken>(true))
                );
        }
        public int VengefulSpiritApplication
        {
            get => DynamicVars[nameof(VengefulSpiritApplication)].IntValue;
            set
            {
                DynamicVars[nameof(VengefulSpiritApplication)].BaseValue = value;
            }
        }

        public override int? GetVengefulSpiritStacksApplied(Creature target)
        {
            return VengefulSpiritApplication;
        }
        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");
            await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue).FromCard(this, cardPlay).Targeting(cardPlay.Target)
                .WithHitFx("vfx/vfx_attack_slash")
                .Execute(choiceContext);
            await PowerCmd.Apply<VengefulSpiritPower>(
                choiceContext, cardPlay.Target, 
                DynamicVars[nameof(VengefulSpiritApplication)].IntValue, Owner.Creature, this);

            if (CombatState == null) return;
            CardModel? chosen = await ReleaseCmd.ChooseRelease(choiceContext, this, ReleaseCost);

            if (ReleaseCmd.ChoseRelease(chosen))
            {
                await ReleaseCmd.Release(choiceContext, Owner.Creature, ReleaseCost, this);

                var card = CombatState.CreateCard<DetonateToken>(Owner);
                CardCmd.Upgrade(card);
                await CardPileCmd.AddGeneratedCardToCombat(card, PileType.Hand, Owner);

            }
        }

        //protected override void OnUpgrade()
        //{
        //    base.DynamicVars.Damage.UpgradeValueBy(2);
        //    DynamicVars[nameof(VengefulSpiritApplication)].UpgradeValueBy(1);
        //}


    }
}
