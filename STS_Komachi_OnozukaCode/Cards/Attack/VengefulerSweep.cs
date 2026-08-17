using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Cards.Tokens;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Commands;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Extensions;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Powers.Abilities;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Powers.Spirits;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Cards
{
    public class VengefulerSweep : STS_Komachi_OnozukaCard
    {
        public VengefulerSweep()
            : base(2, CardType.Attack, CardRarity.Uncommon, TargetType.AllEnemies)
        {
            WithDamage(8, 2);
            // Spirits applied
            WithPower<VengefulSpiritPower>(nameof(Value1), 4, 2);
            // Release cost
            WithVar(nameof(ReleaseCost), 8);
            WithKeyword(KomachiKeywords.Release);
            WithTip(typeof(DetonateToken));
        }

        public override int? GetVengefulSpiritStacksApplied(Creature target)
        {
            return Value1;
        }
        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            
            if (CombatState == null) return;

            CardModel? chosen = await ReleaseCmd.ChooseRelease(choiceContext, this, ReleaseCost);

            await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue).FromCard(this, cardPlay).TargetingAllOpponents(base.CombatState)
            .WithHitFx("vfx/vfx_attack_blunt", null, "heavy_attack.mp3")
            .Execute(choiceContext);
            IReadOnlyList<Creature> enemies = base.CombatState.HittableEnemies;
            foreach (Creature enemy in enemies)
            {
                await PowerCmd.Apply<VengefulSpiritPower>(choiceContext, enemy, base.DynamicVars[nameof(Value1)].BaseValue, base.Owner.Creature, this);
            }

            var card = CombatState.CreateCard<DetonateToken>(Owner);
            await CardPileCmd.AddGeneratedCardToCombat(card, PileType.Hand, Owner);


            if (ReleaseCmd.ChoseRelease(chosen))
            {
                await ReleaseCmd.Release(choiceContext, Owner.Creature, ReleaseCost, this);

                await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue).FromCard(this, cardPlay).TargetingAllOpponents(base.CombatState)
                .WithHitFx("vfx/vfx_attack_blunt", null, "heavy_attack.mp3")
                .Execute(choiceContext);
                foreach (Creature enemy in CombatState.Enemies)
                {
                    await PowerCmd.Apply<VengefulSpiritPower>(choiceContext, enemy, Value1, Owner.Creature, this);
                }

                var card2 = CombatState.CreateCard<DetonateToken>(Owner);
                await CardPileCmd.AddGeneratedCardToCombat(card2, PileType.Hand, Owner);

            }
        }
    }
}
