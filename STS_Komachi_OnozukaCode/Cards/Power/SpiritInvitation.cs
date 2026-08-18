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
    public class SpiritInvitation : STS_Komachi_OnozukaCard
    {
        public SpiritInvitation()
            : base(1, CardType.Power, CardRarity.Rare, TargetType.Self)
        {
            WithPower<GuidedSpiritPower>(nameof(Value1), 2, 1);
            WithPower<DivineSpiritPower>(nameof(Value2), 2, 1);
            WithKeyword(KomachiKeywords.Release);
            WithKeyword(KomachiKeywords.Barrier);
        }

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            if (CombatState == null) return;

            await PowerCmd.Apply<GuidedSpiritPower>(choiceContext, Owner.Creature,
                Value1, Owner.Creature, this);
            await PowerCmd.Apply<DivineSpiritPower>(choiceContext, Owner.Creature,
                Value2, Owner.Creature, this);
            await PowerCmd.Apply<SpiritInvitationPower>(choiceContext, Owner.Creature,
                1, Owner.Creature, this);
        }
    }
}
