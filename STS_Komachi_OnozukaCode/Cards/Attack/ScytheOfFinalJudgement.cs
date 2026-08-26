using BaseLib.Abstracts;
using BaseLib.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Cards.Tokens;
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
    public class ScytheOfFinalJudgement : STS_Komachi_OnozukaCard
    {
        public ScytheOfFinalJudgement()
        : base(1, CardType.Attack, CardRarity.Ancient, TargetType.AnyEnemy)
        {
            WithDamage(15, 5);
            WithPower<DistancePower>(nameof(Value1), 1, 1);
            WithKeyword(KomachiKeywords.Displace);
            WithTip(typeof(DistancePower));
        }

        public override int[] GetPossibleDisplacements() => new[] { -4, -3, -2, -1, 0, 1, 2, 3, 4 };

        public override decimal? GetDistanceMultiplierOverride(int distanceLevel)
            => distanceLevel switch { 4 or 5 => DistancePower.GetInverseDamageMultiplier(distanceLevel), _ => null };


        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            Creature target = cardPlay.Target;
            int currentLevel = DistancePower.GetLevel(target);

            await DistanceCmd.ChooseAndDisplace(choiceContext, cardPlay.Target, this, GetPossibleDisplacements());

            await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
                .FromCard(this, cardPlay)
                .Targeting(target)
                .WithHitFx("vfx/vfx_attack_slash")
                .Execute(choiceContext);
        }

        public override List<DanmakuPiece> patterns => [

            ];
    }
}
