using BaseLib.Extensions;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
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
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Powers.Distance;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Powers.Spirits;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Cards.Attack
{
      
    public class EndlessWay : STS_Komachi_OnozukaCard
    {
        public EndlessWay()
        : base(1, CardType.Skill, CardRarity.Common, TargetType.AnyEnemy)
        {
            WithBlock(7, 3);
            WithPower<DistancePower>(nameof(Value1), 2);
            WithKeyword(KomachiKeywords.Displace);
            // Applied debuff per release
            WithPower<VengefulSpiritPower>(nameof(Value2), 8, 2);
            // Applied debuff #2. Make sure to sync up with Value2
            WithVar(nameof(Value3), 16, 4);
            WithVar(nameof(ReleaseCost), 3);
            // Release cost 2
            WithVar(nameof(ReleaseCost2), 6);
        }

        public override int? GetVengefulSpiritStacksApplied(Creature target)
        {
            if (ReleaseCmd.CanReleaseSpirits(Owner.Creature, Value2))
            {
                return Value2;
            }
            return 0;
        }

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");
            CardModel? chosen = await ReleaseCmd.ChooseRelease(choiceContext, this, ReleaseCost, ReleaseCost2);

            await CreatureCmd.LoseBlock(choiceContext, cardPlay.Target, cardPlay.Target.Block, Owner.Creature);

            // Displacement happens before block for certain powers that care about it.
            await DistanceCmd.Displace(choiceContext, cardPlay.Target, Value1, Owner.Creature, this);

            await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);

            if (ReleaseCmd.ChoseRelease(chosen, ReleaseCost, ReleaseCost2, out var cost))
            {
                await ReleaseCmd.Release(choiceContext, Owner.Creature, cost, this);

                await PowerCmd.Apply<VengefulSpiritPower>(choiceContext, cardPlay.Target, 
                    Value2 * (cost / ReleaseCost), Owner.Creature, this);
            }
        }
    }
}
