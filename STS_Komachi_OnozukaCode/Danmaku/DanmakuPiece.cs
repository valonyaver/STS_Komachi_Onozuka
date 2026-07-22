using Godot;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.Core.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Danmaku
{
    public class DanmakuPiece
    {
        public required string SpritePath;

        /// <summary>
        /// Time until pattern starts
        /// </summary>
        public float StartTimeSeconds = 0f;
        /// <summary>
        /// Lifetime of bullet
        /// </summary>
        public GrowthValue LifeSeconds = 5f;
        /// <summary>
        /// How many times bullets are shot.
        /// </summary>
        public int Group = 1;
        /// <summary>
        /// Interval between each group of bullets
        /// </summary>
        public GrowthValue GIntervalSeconds = 0.1f;

        /// <summary>Bullets per group. Rounded to int at use time.</summary>
        public GrowthValue WayCount = 1f;

        public GrowthValue GAngle = 0f;   // center angle of the group's spread, degrees
        public GrowthValue Range = 0f;    // total spread width across WayCount bullets, degrees
        public GrowthValue Scale = 1f;    // 0 is treated as 1

        public bool IsLaser = false;
        public float LaserWidthPixels = 80f;


        /// <summary>
        /// 1 speed unit = 100 px/s. Speed 7.5 crosses the ~750px average gap in ~1s.
        /// </summary>
        public const float PixelsPerSpeedUnit = 100f;
        /// <summary>
        /// Speed in 100 pixels per second. The average distance between a player and enemy is about 750 pixels.
        /// </summary>
        public GrowthValue StartSpeed = 10f;
        public GrowthValue StartAcc = 0f;       // change in speed, per second
        public GrowthValue StartAccAngle = 0f;  // change in angle, degrees per second

        public DanmakuRootType RootType = DanmakuRootType.Shooter;
        public GrowthValue X = 0f;
        public GrowthValue Y = 0f;
        public GrowthValue Radius = 0f;
        public GrowthValue RadiusA = 0f;

        public DanmakuAimType Aim = DanmakuAimType.PerGroup;

        /// <summary>
        /// If true, a bullet from this Piece hitting a target's hitbox is what allows
        /// AttackCommand's damage to proceed. If no Piece in an attack sets this true,
        /// every Piece's bullets gate damage (so a single-Piece attack works with zero config).
        /// Mirrors LBoL's LastWave, minus the multi-hit-tracking baggage — see notes.
        /// </summary>
        public bool GatesDamage = false;

        /// <summary>
        /// If true, this Piece's entire firing sequence (all Groups/Ways) is run once per
        /// entry in `targets`, each time aimed at that specific target. If false (default),
        /// the whole Piece fires once, aimed at targets[0] — the simplest option, and the
        /// only sensible one for a single-target card anyway.
        /// </summary>
        public bool RepeatPerTarget = false;

        /// <summary>
        /// Whether the bullets of this piece spawn shards on impact
        /// </summary>
        public bool spawnShards = false;

        public Color BulletColor = Colors.White;
        /// <summary>
        /// Spawns a trail behind each bullet
        /// </summary>
        public bool TrailEnabled = false;
        /// <summary>
        /// null = matches BulletColor
        /// </summary>
        public Color? TrailColor = null;

        /// <summary>
        /// How many hits will the bullet take before dying?
        /// </summary>
        public int HitAmount = 1;
        /// <summary>
        /// Interval allowed between each hit.
        /// </summary>
        public float HitIntervalSeconds = 0.1f;
        /// <summary>
        /// If hitamount reaches 0, the bullet will not die.
        /// </summary>
        public bool ZeroHitNotDie = false;

        /// <summary>
        /// SFX played each time this piece's bullet/laser lands a hit. Null = no sfx.
        /// </summary>
        public string? OnHitSfx = null;

        /// <summary>
        /// If true, the struck creature plays its hit/flinch reaction animation each time
        /// this piece's bullet or laser connects.
        /// </summary>
        public bool OnHitAnimation = false;
        /// <summary>
        /// Events :)
        /// </summary>
        public List<DanmakuEventTemplate> Events = new();
    }
    /// <summary>
    /// Where the bullets spawn
    /// </summary>
    public enum DanmakuRootType
    {
        /// <summary>
        /// Spawns at the shooter location. Default.
        /// </summary>
        Shooter,
        /// <summary>
        /// Spawns at the target's location.
        /// </summary>
        Target,
        /// <summary>
        /// Spawns at the specified X and Y offsets.
        /// </summary>
        World
    }

    public enum DanmakuAimType
    {
        /// <summary>GAngle is absolute; 0° = right. No target-relative aiming.</summary>
        None,
        /// <summary>Recompute aim-to-target direction fresh for every group.</summary>
        PerGroup,
        /// <summary>Compute aim-to-target once for group 0, reuse for every later group.</summary>
        FirstGroupAim
    }

    /// <summary>
    ///  Default linear behavior:
    ///   value = Base + PerGroup * groupId + PerWay * wayId
    /// Set CustomFunc for anything nonlinear (quadratic, easing, whatever) — it fully
    /// overrides the linear calculation when present.
    /// </summary>
    public class GrowthValue
    {
        public float Base;
        public float PerGroup;
        public float PerWay;
        public Func<int, int, float>? CustomFunc;

        public float Evaluate(int groupId = 0, int wayId = 0)
            => CustomFunc?.Invoke(groupId, wayId) ?? Base + PerGroup * groupId + PerWay * wayId;

        public static implicit operator GrowthValue(float constant) => new() { Base = constant };

        public GrowthValue()
        {
            Base = 0;
            PerGroup = 0;
            PerWay = 0;
        }
        public GrowthValue(float _base)
        {
            Base = _base;
            PerGroup = 0;
            PerWay = 0;
        }
        public GrowthValue(float _base, float perGroup = 0, float perWay = 0)
        {
            Base = _base;
            PerGroup = perGroup;
            PerWay = perWay;
        }
    }

    public static class DanmakuTime
    {
        /// <summary>Convenience for porting values out of frame-based danmaku guides (assumes 60fps).</summary>
        public static float FramesToSeconds(float frames) => frames / 60f;
        /// <summary>
        /// Multiplier for continuous per-frame movement (bullets, impact-VFX tweens) — the
        /// one part of this system that isn't a discrete await, so it can't go through
        /// Cmd.CustomScaledWait and needs its own explicit scaling.
        /// </summary>
        public static float GetContinuousTimeScale()
        {
            FastModeType mode = SaveManager.Instance.PrefsSave.FastMode;
            if (mode == FastModeType.Fast) return 2f;
            if (mode == FastModeType.Instant) return 2f; // Change later if needded
            return 1f;
        }
    }

    public class DanmakuSlashConfig
    {
        public required string SpritePath;
        public float Scale = 1f;
        public int BulletCount = 20;
        public float SweepDurationSeconds = 0.4f;
        /// <summary>How many consecutively-spawned bullets are alive at once — this is what
        /// derives lifetime from spawn cadence.</summary>
        public int VisibleBulletWindow = 10;
        public int HitAmount = 2;
        public float HitIntervalFractionOfLifetime = 0.9f;
        public bool SpawnShards = true;
        public Color Tint = Colors.White;
        /// <summary>Optional small drift along the slash direction after spawning — 0 keeps
        /// bullets stationary, relying purely on sequential spawn position for the sweep
        /// motion, which is what your description implies. Bump this up only if a static
        /// point-cloud reads as too flat once you see it in-game.</summary>
        public float BulletDriftSpeed = 0f;
    }
}
