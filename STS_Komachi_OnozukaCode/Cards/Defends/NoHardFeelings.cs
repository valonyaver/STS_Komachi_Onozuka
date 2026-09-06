using BaseLib.Extensions;
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
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Powers.Abilities;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Powers.Spirits;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Cards
{
      
    public class NoHardFeelings : STS_Komachi_OnozukaCard
    {
        public NoHardFeelings() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.AnyEnemy)
        {
            WithBlock(4, 3);
            WithPower<VengefulSpiritPower>(nameof(Value1), 2);
            // Strength down per value spirits
            WithVar(new IntVar(nameof(Value2), 2));
            WithKeyword(KomachiKeywords.Detonate);

            WithCalculatedVar(nameof(Value3), 0, StrengthDebuff);
        }

        public static decimal StrengthDebuff(CardModel card, Creature? creature)
        {
            if (creature == null) return 0;
            if (card is not NoHardFeelings nhf) return 0;
            var addition = nhf.GetVengefulSpiritStacksApplied(creature).Value;
            if (creature.HasPower<ArtifactPower>()) addition = 0;
            var vs = VengefulSpiritPower.GetTotalVengefulSpiritAmount(creature, addition);
            if (vs == 0) return addition/nhf.Value2;
            return (vs) / nhf.Value2;
        }
        public static decimal StrengthDebuffNoAddition(CardModel card, Creature? creature)
        {
            if (creature == null) return 0;
            if (card is not NoHardFeelings nhf) return 0;
            var vs = VengefulSpiritPower.GetTotalVengefulSpiritAmount(creature, 0);
            if (vs == 0) return 0;
            return (vs) / nhf.Value2;
        }

        public override bool GainsBlock => true;
        public override int? GetVengefulSpiritStacksApplied(Creature target)
        {
            return Value1;
        }
        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            await CreatureCmd.GainBlock(base.Owner.Creature, base.DynamicVars.Block, cardPlay, false);
            await PowerCmd.Apply<VengefulSpiritPower>(choiceContext, target: cardPlay.Target, Value1, Owner.Creature, this);

            Value3 = (int) Math.Round(StrengthDebuffNoAddition(this, cardPlay.Target), MidpointRounding.ToZero);
            CardModel? chosen = await DetonateToken.GetDetonateOption(choiceContext, CombatState, Owner, cardPlay.Target, this);

            var chosenOption = chosen as DetonateToken;
            if (chosenOption != null && !chosenOption.PreventsDetonation)
            {
                var args = await DetonateCmd.Target(choiceContext, cardPlay.Target, this);
                if (args == null) return;
                var tempDebuffAmount = Math.Round(args.TotalCountedAmount / 2, MidpointRounding.ToZero);
                await PowerCmd.Apply<NoHardFeelingsPower>(choiceContext, args.Target, tempDebuffAmount, Owner.Creature, this);

            }
        }

        
    }
}
