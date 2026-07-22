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
    public class TalkativeFerryman : STS_Komachi_OnozukaCard
    {
        public TalkativeFerryman()
            : base(1, CardType.Power, CardRarity.Uncommon, TargetType.Self)
        {
            // Spirits applied every turn.
            WithVar(nameof(Value1), 2, 1);
            // Release cost
            WithVar(nameof(ReleaseCost), 4);
            // Spirits gained from release
            WithPower<GuidedSpiritPower>(nameof(Value3), 6, 2);

            WithKeyword(KomachiKeywords.Release);
        }

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            if (CombatState == null) return;

            await PowerCmd.Apply<TalkativeFerrymanPower>(choiceContext, Owner.Creature,
                Value1, Owner.Creature, this);

            CardModel? chosen = await ReleaseCmd.ChooseRelease(choiceContext, this, ReleaseCost);


            if (ReleaseCmd.ChoseRelease(chosen))
            {
                await ReleaseCmd.Release(choiceContext, Owner.Creature, ReleaseCost, this);

                await PowerCmd.Apply<VeryTalkativeFerrymanPower>(choiceContext, Owner.Creature,
                Value3, Owner.Creature, this);
            }
        }
    }
}
