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
            WithBlock(14, 2);
            WithKeyword(KomachiKeywords.Release);
            WithKeyword(KomachiKeywords.Barrier);
            // Also the release cost
            WithPower<GuidedSpiritPower>(nameof(Value1), 6, 2);
            WithPower<VengefulSpiritPower>(nameof(Value2), 4, 2);
            // release cost amount
            WithVar(nameof(ReleaseCost), 6);
            WithVar(new SummonVar(6));
        }

        public override int? GetVengefulSpiritStacksApplied(Creature target)
        {
            return Value2;
        }

        protected override void OnUpgrade()
        {
            base.DynamicVars.Summon.UpgradeValueBy(2m);
        }

        public override bool GainsBlock => true;
        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            CardModel releaseChoice = await ReleaseCmd.ChooseRelease(choiceContext, this, ReleaseCost);

            await CreatureCmd.GainBlock(base.Owner.Creature, base.DynamicVars.Block, cardPlay, false);
            await PowerCmd.Apply<GuidedSpiritPower>(choiceContext, target: Owner.Creature, Value1, Owner.Creature, this);
            foreach(var enemy in CombatState.HittableEnemies)
            {
                await PowerCmd.Apply<VengefulSpiritPower>(choiceContext, target: enemy, Value2, Owner.Creature, this);
            }

            if (ReleaseCmd.ChoseRelease(releaseChoice))
            {
                await ReleaseCmd.Release(choiceContext, Owner.Creature, ReleaseCost, this);

                await DivineSpiritCmd.Summon(choiceContext, Owner, DynamicVars.Summon.IntValue, this);
            }
        }
        
    }
}
