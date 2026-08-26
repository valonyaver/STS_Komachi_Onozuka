using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.ValueProps;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Commands;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Extensions;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Minions;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Powers.Spirits;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Cards
{
      
    public class EyeOfTheStorm : STS_Komachi_OnozukaCard
    {
        public EyeOfTheStorm() : base(3, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
        {
            WithBlock(10, 3);
            WithVar(new SummonVar(5));
            WithKeyword(KomachiKeywords.Barrier);
            // Also the release cost
            WithPower<GuidedSpiritPower>(nameof(Value1), 5, 2);
            WithPower<VengefulSpiritPower>(nameof(Value2), 3, 2);
            // release cost amount
            //WithVar(nameof(ReleaseCost), 6);
        }
        protected override void OnUpgrade()
        {
            base.DynamicVars.Summon.UpgradeValueBy(2m);
        }

        public override int? GetVengefulSpiritStacksApplied(Creature target)
        {
            return Value2;
        }

        public override bool GainsBlock => true;
        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            await CreatureCmd.GainBlock(base.Owner.Creature, base.DynamicVars.Block, cardPlay, false);
            await DivineSpiritCmd.Summon(choiceContext, Owner, DynamicVars.Summon.IntValue, this);
            await PowerCmd.Apply<GuidedSpiritPower>(choiceContext, target: Owner.Creature, Value1, Owner.Creature, this);
            foreach(var enemy in CombatState.HittableEnemies)
            {
                await PowerCmd.Apply<VengefulSpiritPower>(choiceContext, target: enemy, Value2, Owner.Creature, this);
            }
        }
        
    }
}
