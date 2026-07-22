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
using MegaCrit.Sts2.Core.Models.Powers;
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
    public class WeedCutter : STS_Komachi_OnozukaCard
    {
        public WeedCutter()
            : base(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
        {
            WithDamage(9);
            WithTip(typeof(ManipulateDistanceToken));
            WithKeyword(CardKeyword.Exhaust);
            WithCostUpgradeBy(-1);
        }

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            
            if (CombatState == null) return;

            await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue).FromCard(this, cardPlay).Targeting(cardPlay.Target)
            .WithHitFx("vfx/vfx_attack_slash", null)
            .WithDanmaku(patterns)
            .Execute(choiceContext);
            await CardPileCmd.Draw(choiceContext, Owner);
            var mandist = CombatState.CreateCard<ManipulateDistanceToken>(Owner);
            await CardPileCmd.AddGeneratedCardToCombat(mandist, PileType.Hand, Owner);
        }

        public override List<DanmakuPiece> patterns => 
            [
                new DanmakuPiece
                {
                    SpritePath = "leaf.png".BulletImagePath(),
                    RootType = DanmakuRootType.Target,
                    Aim = DanmakuAimType.None, // We want them to fly in a fixed direction (e.g. Left to Right)
        
                    // --- The Sweep Logic ---
                    Group = 50,                         // Many leaves for a thick flurry
                    GIntervalSeconds = 0.004f,          // Very fast sequence
        
                    // X: Random horizontal position around the enemy
                    X = new GrowthValue { CustomFunc = (g, w) => (GD.Randf() - 0.5f) * 350f },
        
                    // Y: Starts at -200 (Above) and moves to +200 (Below) over the 80 groups
                    // Formula: Start + (TotalDist / GroupCount) * currentGroup
                    Y = new GrowthValue(-200f, 400f / 50f), 

                    // --- The Movement ---
                    // 0 degrees is Right. If the leaves "Pass" top-to-bottom, 
                    // you might want them to fly horizontally while the "Wave" moves down.
                    GAngle = new GrowthValue { CustomFunc = (g, w) => GD.Randf() * 360f },
                    StartSpeed = 8f,
        
                    // This adds a "flutter" so they don't look like rigid bullets
                    //StartAccAngle = new GrowthValue { CustomFunc = (g, w) => (GD.Randf() - 0.5f) * 60f },

                    // --- Visuals ---
                    Scale = new GrowthValue { CustomFunc = (g, w) => 0.3f + (GD.Randf() * 0.4f) },
                    BulletColor = Colors.LightGoldenrod,
                    GatesDamage = false,
                    spawnShards = true,
                    HitAmount = 2
                },
            // Delays the damage gating until the end
            new DanmakuPiece {
                    SpritePath = "leaf.png".BulletImagePath(),
                    RootType = DanmakuRootType.Target, // Spawns on the enemy
                    StartTimeSeconds = 0.300f,
                    Group = 1,
                    Radius = 0,                     
                    GAngle = new GrowthValue { CustomFunc = (g, w) => GD.Randf() * 360f },   
                    StartSpeed = 10f,
                    Scale = 0.6f,
                    BulletColor = Colors.LightGoldenrod,
                    GatesDamage = true,
                }
            ];
    }
}
