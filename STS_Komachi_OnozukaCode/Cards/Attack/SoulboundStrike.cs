using BaseLib.Extensions;
using BaseLib.Utils;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
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
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Powers.Spirits;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Cards.Attack
{
      
    public class SoulboundStrike : STS_Komachi_OnozukaCard
    {
        public SoulboundStrike()
        : base(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
        {
            WithDamage(9, 3);
            WithTags(CardTag.Strike);
            WithTip( new TooltipSource( (c)=>
                HoverTipFactory.FromCard<DetonateToken>())
                );
            WithTip(KomachiKeywords.Detonate);
        }
        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");
            await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue).FromCard(this, cardPlay).Targeting(cardPlay.Target)
                .WithDanmaku(patterns)
                .WithHitFx("vfx/vfx_attack_slash")
                .Execute(choiceContext);

            if (CombatState == null) return;

            var card = CombatState.CreateCard<DetonateToken>(Owner);
            await CardPileCmd.AddGeneratedCardToCombat(card, PileType.Hand, Owner);
        }
        public override List<DanmakuPiece> patterns => [
            new DanmakuPiece
                {
                    SpritePath = "circle_halo.png".BulletImagePath(),
                    StartTimeSeconds = 0.1f,
                    Group = 1,
                    GIntervalSeconds = 0.05f,
                    WayCount = new GrowthValue {Base = 1},
                    GAngle = new GrowthValue(0),
                    StartSpeed = 1f,
                    StartAcc = 14,
                    Scale = 1f,
                    X = 60,
                    LifeSeconds = 3f,
                    BulletColor = StsColors.purple,
                    GatesDamage = true,
                    spawnShards = true
                },
            new DanmakuPiece
                {
                    SpritePath = "pellet.png".BulletImagePath(),
                    Group = 1,
                    GIntervalSeconds = 0.05f,
                    WayCount = new GrowthValue {Base = 1},
                    GAngle = new GrowthValue(40),
                    StartSpeed = 3f,
                    StartAcc = 6,
                    Scale = 1f,
                    X = 0,
                    Radius = 200,
                    RadiusA = -50,
                    LifeSeconds = 3f,
                    BulletColor = StsColors.pink,
                    TrailEnabled = true
                },
            new DanmakuPiece
                {
                    SpritePath = "pellet.png".BulletImagePath(),
                    Group = 1,
                    GIntervalSeconds = 0.05f,
                    WayCount = new GrowthValue {Base = 1},
                    GAngle = new GrowthValue(-40),
                    StartSpeed = 3f,
                    StartAcc = 6,
                    Scale = 1f,
                    X = 0,
                    Radius = 200,
                    RadiusA = 50,
                    LifeSeconds = 3f,
                    BulletColor = StsColors.pink,
                    TrailEnabled = true
                },
            ];
    }
}
