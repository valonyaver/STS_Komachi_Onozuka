using BaseLib.Abstracts;
using BaseLib.Extensions;
using BaseLib.Hooks;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Badges;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Cards;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Commands;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Danmaku;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Extensions;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Extras;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Patches.PowerPatches;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Patches.Previewers;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Powers.Abilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Powers.Spirits
{
    public class VengefulSpiritPower : STS_Komachi_OnozukaPower, IHasSecondAmount, IHasColoredSecondAmount, IPreExtraHoverTips, IHasThirdAmount
    {
        public override PowerType Type => PowerType.Debuff;
        public override PowerStackType StackType => PowerStackType.Counter;
        public override PowerInstanceType InstanceType => PowerInstanceType.InstancedPerApplier;
        public override Color AmountLabelColor => StsColors.purple;
        protected override IEnumerable<DynamicVar> CanonicalVars =>
        [
            new IntVar(nameof(Duration), 3),
            new DamageVar(nameof(VengefulDamage), 2m, ValueProp.Move)
        ];


        public Creature? DamageTarget => Owner; // explodes onto its own owner
        /// <summary>
        /// The base un-modified explosion damage (Amount * 2).
        /// </summary>
        public decimal BaseDamage => Amount * 2m;
        /// <summary>
        /// Previews what the damage should be against the damage target.
        /// </summary>
        public decimal ModifiedDamage => KomachiHelpers.FindDamageDealt(Applier, DamageTarget, BaseDamage, DynamicVars[nameof(VengefulDamage)]);

        /// <summary>
        /// Same as modified damage, but used to get the localization.
        /// Returns the base damage though, since FindDamageDealt only updates the preview amount.
        /// </summary>
        public decimal VengefulDamage
        {
            get
            {
                KomachiHelpers.FindDamageDealt(Applier, DamageTarget, BaseDamage, DynamicVars[nameof(VengefulDamage)]);
                return DynamicVars[nameof(VengefulDamage)].IntValue;
            }
        }
        /// <summary>
        /// Updates the damage number. Probably redundant since third amount already updates it but not harmful.
        /// </summary>
        public void PreExtraHoverTips() => _ = VengefulDamage;

        public int Duration
        {
            get => DynamicVars[nameof(Duration)].IntValue;
            set
            {
                DynamicVars[nameof(Duration)].BaseValue = value;
                PowerExtensions.InvokeSecondAmountChanged(this);
                if (value == 1) Flash();
            }

        }
        public string GetSecondAmount() => Duration.ToString();
        public Color SecondAmountColor => StsColors.blue;
        public bool ShouldEmphasizeSecondAmount => Duration == 1;
        /// <summary>
        /// UI-only. Stacks a hovered card would add, for previewing GetThirdAmount()
        /// without touching real Amount. 0 = not previewing.
        /// </summary>
        public int PendingStacksPreview;

        /// <summary>
        /// The actual damage on the status.
        /// </summary>
        public decimal? GetThirdAmount()
        {
            if (PendingStacksPreview == 0) return ModifiedDamage; // unchanged original behavior
            decimal hypotheticalBase = (Amount + PendingStacksPreview) * 2m;
            var scratch = new DamageVar("VengefulDamagePreview", 0m, ValueProp.Move);
            return KomachiHelpers.FindDamageDealt(Applier, DamageTarget, hypotheticalBase, scratch);
        }
        public bool ShouldRaiseThirdAmount(CardModel? hoveredCard)
        => hoveredCard is STS_Komachi_OnozukaCard k && k.GetVengefulSpiritStacksApplied(Owner) is > 0;

        public override async Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
        {
            if (!participants.Contains(Owner))
            {
                return;
            }

            Duration--;

            if (Duration <= 0)
            {
                await DetonateCmd.Detonate(new ThrowingPlayerChoiceContext(), this);
            }
        }

        // Just in case safety
        private bool _isExploding = false;
        /// <summary>
        /// Reference to the current detonation args.
        /// Gets fed into it from DetonateCMD so that it can provide back the damage result before being removes.
        /// </summary>
        public DetonationEventArgs? currentDetonationArgs;
        public override async Task AfterRemoved(Creature oldOwner)
        {
            if (_isExploding) return;
            _isExploding = true;
            decimal damage = BaseDamage;

            // Animation starrt
            Creature shooter = Applier ?? oldOwner; // fallback if Applier somehow unavailable
            Control? container = oldOwner.GetVfxContainer();

            if (container != null)
            {
                await DanmakuCmd.FireAndWaitForHit(
                    ConvergePattern, shooter, [oldOwner], container);
            }

            // Damage stuff
            var damageResults = await CreatureCmd.Damage(new ThrowingPlayerChoiceContext(), oldOwner, damage, ValueProp.Move, Applier, null, null);
            var dmgResult = damageResults.FirstOrDefault();
            if (currentDetonationArgs != null)
            {
                currentDetonationArgs.damageResult = dmgResult;
            }

            // Animation end
            if (container != null)
            {
#pragma warning disable CS4014 
                TaskHelper.RunSafely(DanmakuCmd.Fire(
                    ResiduePattern, shooter, [oldOwner], container));
#pragma warning restore CS4014 
            }

            await base.AfterRemoved(oldOwner);
        }

        #region Patterns
        static float CircleCount => 12f;
        static float ResidueCount => 16f;

        public static List<DanmakuPiece> ConvergePattern => [
            new DanmakuPiece() {
            SpritePath = "leaf.png".BulletImagePath(),
            RootType = DanmakuRootType.Target,
            GAngle = new GrowthValue { CustomFunc = (group, way) => way * (360f / CircleCount) },
            WayCount = CircleCount,
            Radius = 180f,
            RadiusA = 170f, // face straight back toward center
            StartSpeed = 4f,
            LifeSeconds = 1f,
            Scale = 0.6f,
            BulletColor = StsColors.purple,
            TrailEnabled = true,
            TrailColor = StsColors.purple,
            GatesDamage = true,
            HitAmount = 2,
            HitIntervalSeconds = 0.3f
        }
        ];

        public static List<DanmakuPiece> ResiduePattern => [
            new DanmakuPiece() {
            SpritePath = "leaf.png".BulletImagePath(),
            RootType = DanmakuRootType.Target,
            GAngle = new GrowthValue { CustomFunc = (group, way) => way * (360f / ResidueCount) },
            WayCount = ResidueCount,
            StartTimeSeconds = 0.4f,
            Radius = 50f,        // spawns at the target itself
            RadiusA = 100,
            StartSpeed = 15f,   // fast burst, off-screen quickly
            LifeSeconds = 1f,
            Scale = 0.5f,
            BulletColor = StsColors.purple,
            TrailEnabled = true,
            TrailColor = StsColors.purple,
            ZeroHitNotDie = true
        }
        ];
        #endregion

        /// <summary>
        /// Preview on the healthbar
        /// </summary>
        public override IEnumerable<HealthBarForecastSegment> GetHealthBarForecastSegments(HealthBarForecastContext context)
        {
            decimal dmg = GetThirdAmount()!.Value;
            var enemyBlock = Owner.Block;
            var length = dmg - enemyBlock;
            if (length <= 0m) yield break;

            yield return new HealthBarForecastSegment(
                Amount: (int)length,
                Color: StsColors.purple,
                Direction: HealthBarForecastDirection.FromRight,
                Order: 0,
                OverlayMaterial: null,
                OverlaySelfModulate: null,
                LeftOriginLayout: HealthBarForecastLeftOriginLayout.Chained,
                LeftExclusiveZGroup: 0,
                AffectsHpLabel: true);
        }
    }
}
