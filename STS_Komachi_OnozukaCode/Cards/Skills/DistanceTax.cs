using BaseLib.Abstracts;
using BaseLib.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Saves;
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
    public class DistanceTax : STS_Komachi_OnozukaCard
    {
        public DistanceTax()
            : base(0, CardType.Skill, CardRarity.Common, TargetType.AllEnemies)
        {
            WithPower<WeakPower>(nameof(Value1), 1, 1);
            WithPower<DistanceTaxPower>(nameof(Value2), 2, 2);
            WithPower<VulnerablePower>(nameof(Value3), 1, 1);

            WithTip(typeof(DistancePower));
        }
        public class DistanceTaxPower : CustomTemporaryPowerModelWrapper<DistanceTax, StrengthPower>
        {
            public override string CustomPackedIconPath => $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".PowerImagePath();
            public override string CustomBigIconPath => $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".BigPowerImagePath();

            protected override bool InvertInternalPowerAmount => true;
            public override PowerType Type => PowerType.Debuff;
        }
        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            if (CombatState == null) return;

            // Says "Pay up!"
            TalkCmd.Play(RawExtraDescription1, Owner.Creature,
                MegaCrit.Sts2.Core.Nodes.Vfx.VfxColor.White,
                MegaCrit.Sts2.Core.Nodes.Vfx.VfxDuration.Short);
            foreach (var enemy in CombatState.HittableEnemies)
            {
                switch(DistancePower.GetLevel(enemy))
                {
                    case 1:
                    case 2:
                        await PowerCmd.Apply<WeakPower>
                            (choiceContext, enemy, Value1, Owner.Creature, this
                            );
                        break;
                    case 3:
                        await PowerCmd.Apply<DistanceTaxPower>
                            (choiceContext, enemy, Value2, Owner.Creature, this
                            );
                        break;
                    case 4:
                    case 5:
                        await PowerCmd.Apply<VulnerablePower>
                            (choiceContext, enemy, Value1, Owner.Creature, this
                            );
                        break;
                }
            }
        }
    }
}
