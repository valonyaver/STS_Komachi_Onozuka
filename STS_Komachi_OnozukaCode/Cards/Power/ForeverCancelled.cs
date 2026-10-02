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
    public class ForeverCancelled : STS_Komachi_OnozukaCard
    {
        public ForeverCancelled()
            : base(2, CardType.Power, CardRarity.Rare, TargetType.AnyEnemy)
        {
            // Spirits applied
            WithPower<VengefulSpiritPower>(nameof(Value1), 8, 6);
            WithTip(KomachiKeywords.Detonate);

            // Percent of spirits refunded
            WithVar(nameof(Value2), 50);
        }

        public override int? GetVengefulSpiritStacksApplied(Creature target)
        {
            return Value1;
        }

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            await PowerCmd.Apply<ForeverCancelledPower>(choiceContext, Owner.Creature,
                Value2, Owner.Creature, this);

            if (CombatState == null) return;
            await PowerCmd.Apply<VengefulSpiritPower>(choiceContext, cardPlay.Target, base.DynamicVars[nameof(Value1)].BaseValue, base.Owner.Creature, this);
        }
    }
}
