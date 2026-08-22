using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
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
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Powers.Distance;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Powers.Spirits;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Cards
{
      
    public class FerriageDeepFog : STS_Komachi_OnozukaCard
    {
        public FerriageDeepFog() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.AnyEnemy)
        {
            // Displace by up to
            WithVar(nameof(Value1), 3);
            // Spirits per displacement
            WithPower<VengefulSpiritPower>(nameof(Value2), 3, 1);
            // Release cost yah
            WithVar(nameof(ReleaseCost), 3);
            // Strength per vengeful spiits
            WithVar(nameof(Value3), 3);
            WithTip(typeof(DistancePower));
            WithKeywords(KomachiKeywords.Displace, KomachiKeywords.Release);
        }
        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            int[] list = [-3, -2, -1];
            var displacement = await DistanceCmd.ChooseAndDisplace(choiceContext, cardPlay.Target, this, list);
            
            await PowerCmd.Apply<VengefulSpiritPower>(choiceContext, target: cardPlay.Target, displacement.ChangeAbs * Value2, Owner.Creature, this);

            
            var vengefulCount = cardPlay.Target?.GetPower<VengefulSpiritPower>()?.Amount;

            if (vengefulCount == null || vengefulCount.Value < 3) return;

            var releaseChoice = await ReleaseCmd.ChooseRelease(choiceContext, this, ReleaseCost);
            if (ReleaseCmd.ChoseRelease(releaseChoice))
            {
                await ReleaseCmd.Release(choiceContext, Owner.Creature, ReleaseCost, this);

                var Fp = vengefulCount.Value / Value3;
                await PowerCmd.Apply<DeepFogPower>(choiceContext, target: Owner.Creature, Fp, Owner.Creature, this);
                
            }
        }

        public class DeepFogPower : CustomTemporaryPowerModelWrapper<FerriageDeepFog, StrengthPower>
        {
            public override PowerType Type => PowerType.Buff;
        }
    }
}
