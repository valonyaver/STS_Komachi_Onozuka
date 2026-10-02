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
    public class BoundSpiritsOfEarth : STS_Komachi_OnozukaCard
    {
        public BoundSpiritsOfEarth()
            : base(2, CardType.Skill, CardRarity.Rare, TargetType.AllEnemies)
        {
            // Spirits applied immediately
            WithPower<VengefulSpiritPower>(nameof(Value1), 18, 6);
        }

        public override int? GetVengefulSpiritStacksApplied(Creature target)
        {
            return Value1;
        }

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            if (CombatState == null) return;
            IReadOnlyList<Creature> enemies = CombatState.HittableEnemies;
            foreach (Creature enemy in enemies)
            {
                await PowerCmd.Apply<VengefulSpiritPower>(choiceContext, enemy, DynamicVars[nameof(Value1)].BaseValue, Owner.Creature, this);
                var vs = enemy.GetPower<VengefulSpiritPower>();
                if (vs != null)
                {
                    vs.Duration = 1;
                }
            }
        }
    }
}
