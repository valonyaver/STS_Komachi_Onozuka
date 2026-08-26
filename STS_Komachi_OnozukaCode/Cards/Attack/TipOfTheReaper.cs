using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Encounters;
using MegaCrit.Sts2.Core.Models.Monsters;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.ValueProps;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Cards.Tokens;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Commands;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Danmaku;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Extensions;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Extras;
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
    public class TipOfTheReaper : STS_Komachi_OnozukaCard
    {
        public TipOfTheReaper()
            : base(0, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
        {
            WithDamage(6, 3);
            WithKeyword(CardKeyword.Ethereal);
            WithTip(typeof(ManipulateDistanceToken));
        }

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            if (CombatState == null) return;

            await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue).FromCard(this, cardPlay).Targeting(cardPlay.Target)
                .WithDanmaku(patterns, 0.5f)
                .WithHitFx("vfx/vfx_attack_slash")
                .Execute(choiceContext);
            float scale = 0.5f;
            NGroundFireVfx nGroundFireVfx = NGroundFireVfx.Create(cardPlay.Target);
            if (nGroundFireVfx == null)
            {
                return;
            }

            SfxCmd.Play("event:/sfx/characters/attack_fire");
            nGroundFireVfx.Scale = Vector2.One * scale;
            NCombatRoom.Instance?.CombatVfxContainer.AddChildSafely(nGroundFireVfx);
        }

        public override async Task AfterCardExhausted(PlayerChoiceContext choiceContext, CardModel card, bool causedByEthereal)
        {
            if (card == this)
            {
                await ManipulateDistanceToken.CreateInHand(Owner, CombatState);
            }
        }

        /// <summary>
        /// Sweep like Pattern
        /// </summary>
        public override List<DanmakuPiece> patterns => [
            new DanmakuPiece {
                SpritePath = "pellet2.png".BulletImagePath(),
                RootType = DanmakuRootType.Target,
                Aim = DanmakuAimType.None,
                WayCount = 4,
                Range = 50,             
                Radius = 200,
                GAngle = -90,          
                RadiusA = 90,           
                StartSpeed = 12,
                Events = [
                    DanmakuEvents.Angle(160, start: 0f, duration: 0.25f)
                ],
                LifeSeconds = 0.5f,
                HitAmount = 1,
                HitIntervalSeconds = 0.25f,
                HitIntervalGatesFirstHit = true,
                TrailEnabled = true,
                spawnShards = true,
                GatesDamage = true,
                BulletColor = StsColors.red,
            }
        ];
    }
}
