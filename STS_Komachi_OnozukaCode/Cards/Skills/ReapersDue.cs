using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Potions;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using MegaCrit.Sts2.GameInfo.Objects;
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
      
    public class ReapersDue : STS_Komachi_OnozukaCard
    {
        protected override bool HasEnergyCostX => true;
        public ReapersDue() : base(0, CardType.Skill, CardRarity.Rare, TargetType.AllEnemies)
        {
            // Over 3 turns if 3: 6 damage at 1. 15 Damage at 2. 24 at 3.
            // Over 3 turns if 4: 9 damage at 1. 21 damage at 2, 33 at 3.
            WithPower<GuidedSpiritPower>(nameof(Value1), 4, 1);
            // Over 3 turns: 8 AOE Damage
            WithPower<VengefulSpiritPower>(nameof(Value2), 8, 2);
            // Over 3 turns: 14 Block.
            WithPower<DivineSpiritPower>(nameof(Value3), 8, 2);
            WithKeyword(KomachiKeywords.Barrier);
            // At 2, deals 15 damage+10 Aoe (25 total)
            // At 3, deals 24 damage+10 aoe + 14 block.

            WithEnergy(1);
            WithVar(new EnergyVar("Energy2", 2));
            WithVar(new EnergyVar("Energy3", 3));

        }

        public override int? GetVengefulSpiritStacksApplied(Creature target)
        {
            return Owner.PlayerCombatState?.Energy * Value2;
        }

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            var X = ResolveEnergyXValue();


            if (X > 0)
            {
                await PowerCmd.Apply<GuidedSpiritPower>(choiceContext, Owner.Creature, Value1 * X, Owner.Creature, this);
            }
            if (X > 1)
            {
                foreach(var enemy in CombatState.HittableEnemies)
                {
                    var Amount = X / 2;
                    await PowerCmd.Apply<VengefulSpiritPower>(choiceContext, enemy, Value2 * Amount, Owner.Creature, this);
                }
            }
            if (X > 2)
            {
                var Amount = X / 3;
                await PowerCmd.Apply<DivineSpiritPower>(choiceContext, Owner.Creature, Value3 * Amount, Owner.Creature, this);
            }
        }
    }
}
