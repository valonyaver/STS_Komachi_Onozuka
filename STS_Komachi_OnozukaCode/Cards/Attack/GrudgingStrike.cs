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
      
    public class GrudgingStrike : STS_Komachi_OnozukaCard
    {
        public GrudgingStrike()
        : base(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
        {
            WithDamage(7, 2);
            WithPower<VengefulSpiritPower>(nameof(VengefulSpiritApplication), 5, 2);
            WithVar(nameof(ReleaseCost), 3, -1);
            WithTags(CardTag.Strike);
            WithTip( new TooltipSource( (c)=>
                HoverTipFactory.FromCard<DetonateToken>(true))
                );
            WithKeyword(KomachiKeywords.Release);
        }
        public int VengefulSpiritApplication
        {
            get => DynamicVars[nameof(VengefulSpiritApplication)].IntValue;
            set
            {
                DynamicVars[nameof(VengefulSpiritApplication)].BaseValue = value;
            }
        }

        public override int? GetVengefulSpiritStacksApplied(Creature target)
        {
            return VengefulSpiritApplication;
        }
        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");
            await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue).FromCard(this, cardPlay).Targeting(cardPlay.Target)
                .WithDanmaku(patterns)
                .WithHitFx("vfx/vfx_attack_slash")
                .Execute(choiceContext);
            await PowerCmd.Apply<VengefulSpiritPower>(
                choiceContext, cardPlay.Target, 
                DynamicVars[nameof(VengefulSpiritApplication)].IntValue, Owner.Creature, this);

            if (CombatState == null) return;
            CardModel? chosen = await ReleaseCmd.ChooseRelease(choiceContext, this, ReleaseCost);

            if (ReleaseCmd.ChoseRelease(chosen))
            {
                await ReleaseCmd.Release(choiceContext, Owner.Creature, ReleaseCost, this);

                var card = CombatState.CreateCard<DetonateToken>(Owner);
                CardCmd.Upgrade(card);
                await CardPileCmd.AddGeneratedCardToCombat(card, PileType.Hand, Owner);

            }
        }

        public GrowthValue startTime => new GrowthValue(1.2f, 0.02f);
        public override List<DanmakuPiece> patterns => [
            new DanmakuPiece() {
                SpritePath = "butterfly.png".BulletImagePath(),
                BulletColor = StsColors.blue,
                Aim = DanmakuAimType.PerGroup,
                Group = 20,
                GIntervalSeconds = 0.1f,
                X = new GrowthValue(80) { CustomFunc = (g, w) => 
                    {
                        float advancePerGroup = 120;
                        float offset = 50;
                        return Mathf.Max(100, g * advancePerGroup + (float)GD.RandRange(-offset, offset));
                    }
                },
                Y = new GrowthValue(50) { CustomFunc = (g, w) =>
                    {
                        float offset = 50;
                        return (float)GD.RandRange(-offset, offset);
                    }
                },
                StartSpeed = 0,
                RadiusA = GrowthValue.RandomGrowthValue(0, -30),
                Radius = GrowthValue.RandomGrowthValue(0, 100, 200),
                Range = 180,
                WayCount = 3,
                Scale = 0.6f,
                LifeSeconds = 1,
                ZeroHitNotDie = true,
                spawnShards = true,
                ExpandOnSpawn = true,
            },
            //new DanmakuPiece() {
            //    SpritePath = "butterfly.png".BulletImagePath(),
            //    BulletColor = StsColors.blue,
            //    Aim = DanmakuAimType.FirstGroupAim,
            //    Range = 60,
            //    WayCount = 3,
            //    Group = 8,
            //    GIntervalSeconds = 0f,
            //    GAngle = new GrowthValue(10),
            //    StartAccAngle = -60,
            //    StartSpeed = new GrowthValue(4f),
            //    Scale = 0.6f,
            //    Events = [
            //        DanmakuEvents.Homing(360, startTime, 0, true),
            //        DanmakuEvents.Speed(new GrowthValue(6, 1), startTime, 0),
            //        DanmakuEvents.AccelerationAngle(0, startTime, 0, DanmakuEventMode.Transition)
            //        ]
            //},
            new DanmakuPiece
                {
                    SpritePath = "circle_halo.png".BulletImagePath(),
                    StartTimeSeconds = startTime.Base,
                    Group = 1,
                    GIntervalSeconds = 0.1f,
                    WayCount = new GrowthValue {Base = 1},
                    GAngle = new GrowthValue(0),
                    StartSpeed = 12f,
                    //Events = [
                    //    DanmakuEvents.Speed(6, startTime, 0)
                    //    ],
                    Scale = 1.3f,
                    X = 60,
                    LifeSeconds = 3f,
                    BulletColor = StsColors.purple,
                    GatesDamage = true,
                    spawnShards = true,
                    
                },
            ];
    }
}
