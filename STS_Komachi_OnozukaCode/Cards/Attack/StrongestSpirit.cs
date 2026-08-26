using BaseLib.Extensions;
using BaseLib.Utils;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.ValueProps;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Commands;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Danmaku;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Extensions;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Powers.Spirits;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Cards.Attack
{
      
    public class StrongestSpirit : STS_Komachi_OnozukaCard
    {
        public StrongestSpirit()
        : base(2, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy)
        {
            WithDamage(9);
            WithPower<VengefulSpiritPower>(nameof(Value1), 9);
            WithPower<StrengthPower>(nameof(Value2), 9);
            WithTip(typeof(DexterityPower));
            WithKeyword(CardKeyword.Retain, UpgradeType.Add);
            WithVar(nameof(ReleaseCost), 9);
            WithKeyword(KomachiKeywords.Release);
        }

        public override int? GetVengefulSpiritStacksApplied(Creature target)
        {
            return Value1;
        }
        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");
            AttackCommand attackCommand = await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue).FromCard(this, cardPlay).Targeting(cardPlay.Target)
                .WithHitFx("vfx/vfx_attack_slash")
                .Execute(choiceContext);
            await PowerCmd.Apply<VengefulSpiritPower>(choiceContext, cardPlay.Target, Value1, base.Owner.Creature, this);

            // Don't do release if target is dead.
            if (cardPlay.Target.IsDead) return;
            var choiceRelease = await ReleaseCmd.ChooseRelease(choiceContext, this, ReleaseCost);

            if (ReleaseCmd.ChoseRelease(choiceRelease))
            {
                await ReleaseCmd.Release(choiceContext, Owner.Creature, ReleaseCost, this);

                var handPile = Owner.PlayerCombatState.Hand.Cards.ToList();
                int amount = 0;
                foreach(var card in handPile)
                {
                    await CardCmd.Exhaust(choiceContext, card);
                    amount++;
                    
                }

                float scale = 0.8f;
                await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue).WithHitCount(amount).FromCard(this, cardPlay)
                .Targeting(cardPlay.Target)
                .BeforeDamage(delegate
                {
                NGroundFireVfx nGroundFireVfx = NGroundFireVfx.Create(cardPlay.Target);
                if (nGroundFireVfx == null)
                {
                    return Task.CompletedTask;
                }

                SfxCmd.Play("event:/sfx/characters/attack_fire");
                nGroundFireVfx.Scale = Vector2.One * scale;
                NCombatRoom.Instance?.CombatVfxContainer.AddChildSafely(nGroundFireVfx);
                scale += 0.1f;
                return Task.CompletedTask;
                })
                .Execute(choiceContext);
                MainFile.LogMessage($"Amount of exiled cards is {amount}");
                if (amount >= 9)
                {
                    await PowerCmd.Apply<StrengthPower>(choiceContext, Owner.Creature, Value2, base.Owner.Creature, this);
                    // await PowerCmd.Apply<DexterityPower>(choiceContext, Owner.Creature, Value2, base.Owner.Creature, this);
                }
            }
        }

        //protected override void OnUpgrade()
        //{
        //    base.DynamicVars.Damage.UpgradeValueBy(2);
        //    DynamicVars[nameof(VengefulSpiritApplication)].UpgradeValueBy(1);
        //}

        public override List<DanmakuPiece> patterns => [

            ];
    }
}
