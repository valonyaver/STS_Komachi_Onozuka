using BaseLib.Abstracts;
using BaseLib.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.ValueProps;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Cards.Tokens;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Commands;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Danmaku;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Extensions;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Extras;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Powers.Distance;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Powers.Spirits;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Cards.Attack
{
      
    public class ShootAndMove : STS_Komachi_OnozukaCard, ITranscendenceCard
    {
        public ShootAndMove()
        : base(1, CardType.Attack, CardRarity.Basic, TargetType.AnyEnemy)
        {
            WithDamage(8, 2);
            WithPower<DistancePower>(nameof(Value1), 1, 1);
            WithKeyword(KomachiKeywords.Displace);
        }

        public CardModel GetTranscendenceTransformedCard()
        {
            return ModelDb.Card<ScytheOfFinalJudgement>();
        }

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");
            await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this, cardPlay).Targeting(cardPlay.Target)
                .WithHitFx("vfx/vfx_attack_slash")
                .WithDanmaku(patterns)
                .Execute(choiceContext);
            await Cmd.CustomScaledWait(0.5f, 0.7f);
            if (CombatState == null) return;
            
            int[] options = IsUpgraded ? new[] { -2, -1, 0, 1, 2 } : new[] { -1, 0, 1 };
            await DistanceCmd.ChooseAndDisplace(choiceContext, cardPlay.Target, this, options);
        }

        public override List<DanmakuPiece> patterns => [
                new DanmakuPiece
                {
                    SpritePath = "coin.png".BulletImagePath(),
                    Group = 5,
                    GIntervalSeconds = 0.05f,
                    WayCount = new GrowthValue {Base = 1},
                    GAngle = new GrowthValue(0),
                    StartSpeed = 14f,
                    Scale = 0.5f,
                    X = 60,
                    LifeSeconds = 3f,
                    BulletColor = StsColors.gold,
                },
                new DanmakuPiece
                    {
                        SpritePath = "coin.png".BulletImagePath(),
                        StartTimeSeconds = 0.20f,
                        Group = 5,
                        GIntervalSeconds = 0.05f,
                        WayCount = new GrowthValue {Base = 1},
                        GAngle = new GrowthValue(0, -5),
                        StartSpeed = 14f,
                        Scale = 0.5f,
                        X = 60,
                        LifeSeconds = 3f,
                        BulletColor = StsColors.gold,
                    },
                new DanmakuPiece
                    {
                        SpritePath = "coin.png".BulletImagePath(),
                        StartTimeSeconds = 0.5f,
                        Group = 10,
                        GIntervalSeconds = 0.025f,
                        WayCount = new GrowthValue {Base = 1},
                        GAngle = new GrowthValue(-25, 5),
                        StartSpeed = 14f,
                        Scale = 0.5f,
                        X = 60,
                        LifeSeconds = 3f,
                        BulletColor = StsColors.gold,
                    },
            ];
    }
}
