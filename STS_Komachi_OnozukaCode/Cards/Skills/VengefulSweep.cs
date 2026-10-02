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
    public class VengefulSweep : STS_Komachi_OnozukaCard
    {
        public VengefulSweep()
            : base(1, CardType.Skill, CardRarity.Common, TargetType.AllEnemies)
        {
            // Spirits applied
            WithPower<VengefulSpiritPower>(nameof(Value1), 10, 4);
            // Release cost
            WithVar(nameof(ReleaseCost), 4);
            WithKeyword(KomachiKeywords.Release);
        }

        public override int? GetVengefulSpiritStacksApplied(Creature target)
        {
            return Value1;
        }

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            if (CombatState == null) return;

            CardModel? chosen = await ReleaseCmd.ChooseRelease(choiceContext, this, ReleaseCost);

            IReadOnlyList<Creature> enemies = base.CombatState.HittableEnemies;
            foreach (Creature enemy in enemies)
            {
                await PowerCmd.Apply<VengefulSpiritPower>(choiceContext, enemy, base.DynamicVars[nameof(Value1)].BaseValue, base.Owner.Creature, this);
            }


            if (ReleaseCmd.ChoseRelease(chosen))
            {
                await ReleaseCmd.Release(choiceContext, Owner.Creature, ReleaseCost, this);

                foreach (Creature enemy in CombatState.HittableEnemies)
                {
                    await PowerCmd.Apply<VengefulSpiritPower>(choiceContext, enemy, Value1, Owner.Creature, this);
                }
            }
        }
    }
}
