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
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.ValueProps;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Commands;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Danmaku;
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
      
    public class SliceOfHigan : STS_Komachi_OnozukaCard, IOnDistanceChangedListener
    {
        public SliceOfHigan()
        : base(1, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy)
        {
            WithDamage(3);
            // Damage increase from displacement
            WithVar(nameof(Value1), 1, 1);
            WithKeyword(KomachiKeywords.Displace);
            WithTip(typeof(DistancePower));
        }

        public async Task OnDistanceChanged(PlayerChoiceContext choiceContext, DistanceChangedEventArgs args)
        {
            if (args.ChangeAbs > 0)
            {
                DynamicVars.Damage.BaseValue += Value1;
            }
        }

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");

            var damageResult = await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue)
                                                .WithHitCount(2)
                                                .FromCard(this, cardPlay)
                                                .Targeting(cardPlay.Target)
                                                .WithHitFx("vfx/vfx_attack_slash")
                                                .Execute(choiceContext);
        }

        public override List<DanmakuPiece> patterns => [

            ];
    }
}
