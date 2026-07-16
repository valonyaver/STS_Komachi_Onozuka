using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.ValueProps;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Danmaku;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Extensions;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Extras;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Cards.Basics
{
      
    public class StrikeKomachi : STS_Komachi_OnozukaCard
    {

        public StrikeKomachi() : base(1, CardType.Attack, CardRarity.Basic, TargetType.AnyEnemy)
        {
            WithTags(CardTag.Strike);
            WithDamage(6, 3);
        }
        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");
            await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this, cardPlay).Targeting(cardPlay.Target)
                .WithDanmaku(patterns)
                .WithHitFx("vfx/vfx_attack_slash")
                .Execute(choiceContext);
        }

        public override List<DanmakuPiece> patterns =>
            [
                new DanmakuPiece
                {
                    SpritePath = "coin.png".BulletImagePath(),
                    Group = 10,
                    GIntervalSeconds = 0.05f,
                    WayCount = new GrowthValue {Base = 1},
                    GAngle = new GrowthValue(15, -5f),
                    StartSpeed = 16f,
                    Scale = 0.5f,
                    X = 60,
                    LifeSeconds = 3f,
                    BulletColor = StsColors.gold,
                }
            ];
    }
}
