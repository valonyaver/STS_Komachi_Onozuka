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
    public class FreeTrauma : STS_Komachi_OnozukaCard
    {
        public FreeTrauma()
            : base(0, CardType.Skill, CardRarity.Common, TargetType.AnyEnemy)
        {
            MainFile.LogMessage("Logging the constructor for Free trauma");

            // Spirits applied
            WithPower<VengefulSpiritPower>(nameof(Value1), 2, 1);
            // Release cost 1
            WithVar(nameof(ReleaseCost), 4, -1);
            // Release cost 2
            WithVar(nameof(Value3), 8, -2);

            WithEnergy(1);

            WithKeyword(KomachiKeywords.Release);
        }

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            if (CombatState == null) return;

            CardModel? chosen = await ReleaseCmd.ChooseRelease(choiceContext, this, ReleaseCost, Value3);

            await PowerCmd.Apply<VengefulSpiritPower>(choiceContext, cardPlay.Target, Value1, base.Owner.Creature, this);



            if (ReleaseCmd.ChoseRelease(chosen, ReleaseCost, Value3, out var cost))
            {
                await ReleaseCmd.Release(choiceContext, Owner.Creature, cost, this);

                await PlayerCmd.GainEnergy(cost / ReleaseCost, Owner);
            }
        }
    }
}
