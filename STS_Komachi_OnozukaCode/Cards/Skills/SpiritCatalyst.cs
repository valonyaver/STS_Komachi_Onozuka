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
    public class SpiritCatalyst : STS_Komachi_OnozukaCard
    {
        public SpiritCatalyst()
            : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.AnyEnemy)
        {
            // Spirits applied
            WithPower<VengefulSpiritPower>(nameof(Value1), 0);

            WithKeyword(CardKeyword.Retain, UpgradeType.Add);
            WithKeyword(CardKeyword.Exhaust);
        }

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            if (CombatState == null) return;

            int num = (cardPlay.Target.IsAlive ? cardPlay.Target.GetPowerAmount<VengefulSpiritPower>() : 0);
            if (num > 0)
            {
                await PowerCmd.Apply<VengefulSpiritPower>(choiceContext, cardPlay.Target, num, base.Owner.Creature, this);
            }
        }
    }
}
