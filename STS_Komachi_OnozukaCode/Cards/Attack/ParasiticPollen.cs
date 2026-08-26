using BaseLib.Extensions;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Cards.Tokens;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Danmaku;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Powers.Spirits;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Cards.Attack
{
      
    public class ParasiticPollen : STS_Komachi_OnozukaCard
    {
        public ParasiticPollen()
        : base(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
        {
            WithDamage(4, -1);
            // Attack times
            WithVar(nameof(Value1), 2, 1);
            WithPower<PoisonPower>(nameof(Value2), 3, 1);
            WithTip(typeof(SpiderLily));
        }
        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");
            await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue).FromCard(this, cardPlay).Targeting(cardPlay.Target)
                .WithHitCount(Value1)
                .WithHitFx("vfx/vfx_attack_slash")
                .Execute(choiceContext);

            CardModel lily = CombatState.CreateCard<SpiderLily>(Owner);
            CardCmd.PreviewCardPileAdd(await CardPileCmd.AddGeneratedCardToCombat(lily, PileType.Draw, Owner, CardPilePosition.Random));
            
            if (Owner.HasPower<PoisonPower>())
            {
                var poison = Owner.Creature.GetPower<PoisonPower>();
                var poisonAmount = Owner.Creature.GetPowerAmount<PoisonPower>();

                var reduction = Math.Min(poisonAmount, Value2);

                await PowerCmd.ModifyAmount(choiceContext, poison, -reduction, Owner.Creature, this);

                await PowerCmd.Apply<PoisonPower>(choiceContext, cardPlay.Target, reduction * 2, Owner.Creature, this);
            }
        }


        public override List<DanmakuPiece> patterns => [

            ];
    }
}
