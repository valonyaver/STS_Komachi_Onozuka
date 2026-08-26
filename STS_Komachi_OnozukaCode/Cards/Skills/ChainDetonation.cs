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
      
    public class ChainDetonation : STS_Komachi_OnozukaCard
    {
        public ChainDetonation()
            : base(2, CardType.Skill, CardRarity.Rare, TargetType.AnyEnemy)
        {
            // Spirits applied
            WithPower<VengefulSpiritPower>(nameof(Value1), 4, 2);
            // Release cost 1
            WithVar(nameof(ReleaseCost), 3, -1);
            // Release cost 2
            WithVar(nameof(Value3), 8, -1);


            WithKeyword(KomachiKeywords.Release);
            WithKeyword(KomachiKeywords.Detonate);

        }

        public override int? GetVengefulSpiritStacksApplied(Creature target)
        {
            return Value1;
        }

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            if (CombatState == null) return;

            
            // Apply the first batch.
            await PowerCmd.Apply<VengefulSpiritPower>(choiceContext, cardPlay.Target, Value1, Owner.Creature, this);

            // Detonate it.
            DetonationEventArgs? boom1 = await DetonateCmd.Target(choiceContext, cardPlay.Target, this);

            if (cardPlay.Target.IsDead) return;
            // If detonation successful, apply again.
            if (DetonateCmd.IsDetonationSuccessful(boom1))
            {
                await PowerCmd.Apply<VengefulSpiritPower>(choiceContext, cardPlay.Target, boom1.TotalCountedAmount, Owner.Creature, this);
            }

            if (cardPlay.Target.IsDead) return;
            CardModel? chosen = await ReleaseCmd.ChooseRelease(choiceContext, this, ReleaseCost, Value3);


            if (ReleaseCmd.ChoseRelease(chosen, ReleaseCost, Value3, out var cost))
            {
                await ReleaseCmd.Release(choiceContext, Owner.Creature, cost, this);

                // If releasing, detonate again.
                DetonationEventArgs? boom2 = await DetonateCmd.Target(choiceContext, cardPlay.Target, this);

                // If releasing high, apply again.
                if (ReleaseCmd.ChoseSecondRelease(chosen) && DetonateCmd.IsDetonationSuccessful(boom2))
                {
                    await PowerCmd.Apply<VengefulSpiritPower>(choiceContext, cardPlay.Target, boom2.TotalCountedAmount, Owner.Creature, this);
                }
            }
        }
    }
}
