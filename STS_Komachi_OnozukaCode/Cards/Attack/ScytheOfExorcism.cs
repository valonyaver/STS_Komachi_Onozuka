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
    public class ScytheOfExorcism : STS_Komachi_OnozukaCard
    {
        public ScytheOfExorcism()
            : base(2, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
        {
            WithDamage(14, 4);
            // Release cost
            WithVar(nameof(Value2), 4, -1);
            WithKeyword(KomachiKeywords.Release);
            WithKeyword(KomachiKeywords.Detonate);
            WithTip(typeof(SpiderLily));
            // Spirits needed
            WithPower<VengefulSpiritPower>(nameof(Value1), 4);
            WithTip(typeof(ArtifactPower));
        }

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            
            if (CombatState == null) return;

            CardModel? chosen = await ReleaseCmd.ChooseRelease(choiceContext, this, Value2);
            if (ReleaseCmd.ChoseRelease(chosen))
            {
                await ReleaseCmd.Release(choiceContext, Owner.Creature, Value2, this);

                await PowerCmd.Apply<VengefulSpiritPower>(choiceContext, cardPlay.Target, Value1, Owner.Creature, this);
                await PowerCmd.Apply<ArtifactPower>(choiceContext, Owner.Creature, 1, Owner.Creature, this);
            }

            await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue).
                FromCard(this, cardPlay).Targeting(cardPlay.Target)
                .WithHitFx("vfx/vfx_attack_blunt", null, "heavy_attack.mp3")
                .Execute(choiceContext);

            DetonateToken choice = (DetonateToken)await DetonateToken.
                                    GetDetonateOption(choiceContext, CombatState, Owner, cardPlay.Target);
            
            if (choice != null && !choice.PreventsDetonation)
            {
                var detonate = await DetonateCmd.Target(choiceContext, cardPlay.Target, this);
                if (detonate.TotalCountedAmount >= Value1)
                {
                    CardModel lily = CombatState.CreateCard<SpiderLily>(Owner);
                    await CardPileCmd.AddGeneratedCardToCombat(lily, PileType.Hand, Owner);
                }
            }
        }
    }
}
