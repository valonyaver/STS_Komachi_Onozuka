using Godot;
using MegaCrit.Sts2.Core.Audio.Debug;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
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
using MegaCrit.Sts2.Core.Nodes.Audio;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.ValueProps;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Cards.Tokens;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Commands;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Configs;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Danmaku;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Danmaku.Nodes;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Extensions;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Extras;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Powers.Abilities;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Powers.Spirits;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Cards
{
    public class ShortLifeExpectancy : STS_Komachi_OnozukaCard
    {
        public ShortLifeExpectancy()
            : base(3, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy)
        {
            WithCalculatedDamage(0, GetEnemyHP);
            // Normal Release cost
            WithVar(nameof(Value1), 10, -2);
            // Boss+Elite Release cost
            WithVar(nameof(Value2), 20, -4);
            WithKeyword(KomachiKeywords.Release);
            WithKeyword(CardKeyword.Exhaust);

            WithVar(nameof(ReleaseCost), 10, -2);
        }

        protected override bool ShouldGlowGoldInternal
        {
            get
            {
                if (CombatState == null) return false;

                if (!IsEliteRoom())
                {
                    return ReleaseCmd.CanReleaseSpirits(Owner.Creature, Value1);
                }

                // In Elite/Boss rooms: secondary enemies (minions) cost Value1.
                // Glow if one of those exist
                return CombatState.HittableEnemies.Any(enemy =>
                    ReleaseCmd.CanReleaseSpirits(
                        Owner.Creature,
                        enemy.IsSecondaryEnemy ? Value1 : Value2
                    )
                );
            }
        }


        public static decimal GetEnemyHP(CardModel card, Creature? creature)
        {
            if (creature == null) return 0;
            return creature.CurrentHp / 2;
        }


        public bool IsEliteRoom()
        {
            if (Owner.RunState == null) return false;
            if (Owner.RunState.CurrentRoom == null) return false;

            var room = Owner.RunState.CurrentRoom.RoomType;
            bool isElite = (Owner.RunState.CurrentRoom.RoomType == RoomType.Elite 
                            || Owner.RunState.CurrentRoom.RoomType == RoomType.Boss);
            return isElite;
        }

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            if (CombatState == null) return;
            var isElite = IsEliteRoom() && !cardPlay.Target.IsSecondaryEnemy;
            MainFile.LogMessage($"Is the player in an elite room? {isElite}. The current room is {Owner.RunState.CurrentRoom.RoomType}");
            var releaseCost = isElite ? Value2 : Value1;

            // For extra description purposes
            ReleaseCost = releaseCost;

            CardModel? chosen = await ReleaseCmd.ChooseRelease(choiceContext, this, releaseCost);

            if (ReleaseCmd.ChoseRelease(chosen))
            {
                await ReleaseCmd.Release(choiceContext, Owner.Creature, releaseCost, this);
                float totalPatternTime = TotalSlashTime(flurryCount) + patterns[^1].LifeSeconds.Evaluate();

                SfxCmd.Play("spellcard2.wav".SoundEffectPath());
                if (!KomachiConfigs.SkipDanmaku)
                {
                    NDanmakuDarkenOverlay? darkenVfx = NDanmakuDarkenOverlay.Create(
                        totalPatternTime + NDanmakuDarkenOverlay._introDuration,
                        originCreature: Owner.Creature,
                        style: DarkenOverlayStyle.Expand,
                        tint: new Color(0.1f, 0, 0, 0.5f)
                        );


                    if (darkenVfx != null)
                    {
                        NCombatRoom.Instance?.BgContainer.AddChildSafely(darkenVfx);
                    }


                    await Cmd.Wait(NDanmakuDarkenOverlay._introDuration - NDanmakuDarkenOverlay._introDuration * 0.2f);
                }
                await DamageCmd.Attack(GetEnemyHP(this, cardPlay.Target))
                    .WithHitFx("vfx/vfx_big_slash", null, "slash_attack.mp3")
                    .FromCard(this, cardPlay)
                    .Targeting(cardPlay.Target)
                    .WithDanmaku(patterns, 7)
                    .Execute(choiceContext);
            }
        }
        static float SlashDuration(int group)
        {
            const float start = 1.5f;
            const float floor = 0.1f;
            const float decay = 0.6f; // lower = faster drop-off; tune to taste
            return floor + (start - floor) * Mathf.Pow(decay, group);
        }

        static float TotalSlashTime(int count)
        {
            float total = 0f;
            for (int i = 0; i < count; i++)
                total += SlashDuration(i);
            return total;
        }

        const int flurryCount = 20;
        public override List<DanmakuPiece> patterns
        {
            get
            {
                float finalSlashStart = TotalSlashTime(flurryCount);

                return [
                        // Opening X-slash + random-direction flurry, 10 slashes total
                        new DanmakuPiece() {
                            SpritePath = "danmaku/timestopLaser.tscn".ScenePath(),
                            IsLaser = true,
                            RootType = DanmakuRootType.Target,
                            RadiusA = 180f,
                            Radius = 140f,
                            LaserWidthPixels = 80f,
                            Group = flurryCount,
                            GAngle = new GrowthValue { CustomFunc = (group, way) => group switch {
                                0 => 225f, // top-left
                                1 => 315f, // top-right
                                _ => (float)GD.RandRange(0.0, 360.0),
                            }},
                            LifeSeconds = 0.5f,
                            GIntervalSeconds = new GrowthValue { CustomFunc = (group, way) => SlashDuration(group) },
                            OnHitSfx = "slash_attack.mp3".SoundEffectPath(),
                            HitIntervalSeconds = 0.05f
                        },  
                        // Finishing slash — the one that actually gates damage
                        new DanmakuPiece() {
                            SpritePath = "danmaku/timestopLaser.tscn".ScenePath(),
                            IsLaser = true,
                            RootType = DanmakuRootType.Target,
                            RadiusA = 180f,
                            Radius = 140f,
                            LaserWidthPixels = 100f, // a bit bigger, to read as the "final blow"
                            LifeSeconds = 1f,
                            StartTimeSeconds = finalSlashStart,
                            GatesDamage = true,
                            OnHitSfx = "heavy_attack.mp3".SoundEffectPath()
                        },
                ];
            }
        }
    }
}
