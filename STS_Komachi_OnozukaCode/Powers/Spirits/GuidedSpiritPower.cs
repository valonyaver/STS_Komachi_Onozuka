using BaseLib.Abstracts;
using BaseLib.Extensions;
using Godot;
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
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.ValueProps;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Danmaku;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Extensions;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Extras;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Patches.PowerPatches;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Patches.Previewers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Powers.Spirits
{
    public class GuidedSpiritPower : STS_Komachi_OnozukaPower, IPreExtraHoverTips, IHasThirdAmount
    {
        public override PowerType Type => PowerType.Buff;
        public override PowerStackType StackType => PowerStackType.Counter;
        protected override IEnumerable<DynamicVar> CanonicalVars =>
        [
            new DamageVar("GuidedDamage", 1m, ValueProp.Move),
            new StringVar("targetName", "None")
        ];

        // It targets the enemy with the lowest HP
        public Creature? DamageTarget
        {
            get
            {
                if (CombatState.HittableEnemies.Count <= 0)
                {
                    ((StringVar)DynamicVars["targetName"]).StringValue = "None";
                    return null;
                }
                var t = CombatState.HittableEnemies.OrderBy(c => c.CurrentHp).ToList()[0];
                ((StringVar)DynamicVars["targetName"]).StringValue = t.Name;
                return t;
            }
        }
        public decimal BaseDamage => Amount;

        /// <summary>
        /// Previews what the damage should be against the damage target.
        /// </summary>
        public decimal ModifiedDamage => KomachiHelpers.FindDamageDealt(Owner, DamageTarget, BaseDamage, DynamicVars["GuidedDamage"]);
        public decimal? GetThirdAmount()
        {
            if (DamageTarget == null)
                return null;

            // Truncate to whole integer and clamp at 0
            return (int)Math.Max(ModifiedDamage, 0m);
        }
        public void PreExtraHoverTips() => _ = ModifiedDamage;

        protected override IEnumerable<IHoverTip> ExtraHoverTips
        {
            get {
                PreExtraHoverTips();
                return base.ExtraHoverTips;
            }
        }
        public override async Task BeforeSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
        {
            if (!participants.Contains(Owner))
            {
                return;
            }


            if (DamageTarget != null) {
                Control? container = Owner.GetVfxContainer();
                if (container != null)
                {
                    await DanmakuCmd.FireAndWaitForHit(GetPattern(Amount), Owner, [DamageTarget], container);
                }

                await CreatureCmd.Damage(choiceContext,
                    target: DamageTarget,
                    amount: Amount,
                    props: ValueProp.Move,
                    dealer: Owner);
            }
            Amount--;
            if (Amount <= 0)
            {
                await PowerCmd.Remove(this);
            }
        }

        public static List<DanmakuPiece> GetPattern(int amount)
        {
            int count = Mathf.Max(1, amount);
            // 1 spirit = dead-on aim. More spirits = wider possible spread, capped so it never gets absurd.
            float maxSpreadDeg = Mathf.Min(120f, (count) * 12f);

            return [
                new DanmakuPiece() {
                SpritePath = "circle_outlined.png".BulletImagePath(),
                RootType = DanmakuRootType.Shooter,
                Aim = DanmakuAimType.PerGroup,
                WayCount = count,

                // Random scatter around the shooter's position, independent of firing angle
                X = new GrowthValue { CustomFunc = (group, way) => (float)GD.RandRange(-30.0, 30.0) },
                Y = new GrowthValue { CustomFunc = (group, way) => (float)GD.RandRange(-30.0, 30.0) },

                // Random angle offset from dead-center-aim, scaling with count
                GAngle = new GrowthValue { CustomFunc = (group, way) =>
                    (float)GD.RandRange(-maxSpreadDeg / 2.0, maxSpreadDeg / 2.0) },

                StartSpeed = new GrowthValue { CustomFunc = (group, way) => (float)GD.RandRange(8.0, 9)},
                LifeSeconds = 3.5f,
                Scale = 0.6f,
                BulletColor = Colors.LightPink,
                TrailEnabled = true,

                Events = [
                    // Brief straight flight before it starts curving toward the target
                    DanmakuEvents.Homing(
                        turnSpeedDegPerSec: new GrowthValue { CustomFunc = (group, way) => (float)GD.RandRange(80, 100) },
                        start: new GrowthValue { CustomFunc = (group, way) => (float)GD.RandRange(0.1, 0.2) },
                        duration: 3f)
                ],
            },
        ];
        }
    } 
}
