using Godot;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Danmaku.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Danmaku
{
    public class DanmakuEvent
    {
        public float Start;
        public float Duration;
        public bool HasStarted;
        public bool HasFinished;
        public Action<NDanmakuBullet>? OnStart;
        /// <summary>(bullet, elapsedSinceEventStart, normalizedProgress 0..1, dt)</summary>
        public required Action<NDanmakuBullet, float, float, float> Apply;
    }

    public class DanmakuEventTemplate
    {
        /// <summary>Resolves this template into a concrete runtime event for one specific bullet's (group, way).</summary>
        public required Func<int, int, DanmakuEvent> Resolve;
    }

    public enum DanmakuEventMode { Add, Transition, Multiply }

    public static class DanmakuEvents
    {
        public static DanmakuEventTemplate Speed(GrowthValue amount, GrowthValue start, GrowthValue duration, DanmakuEventMode mode = DanmakuEventMode.Add)
            => Float(amount, start, duration, mode, b => b.Speed, (b, v) => b.Speed = v);

        public static DanmakuEventTemplate Angle(GrowthValue amountDeg, GrowthValue start, GrowthValue duration, DanmakuEventMode mode = DanmakuEventMode.Add)
            => Float(amountDeg, start, duration, mode, b => b.AngleDeg, (b, v) => b.AngleDeg = v);

        public static DanmakuEventTemplate Acceleration(GrowthValue amount, GrowthValue start, GrowthValue duration, DanmakuEventMode mode = DanmakuEventMode.Add)
            => Float(amount, start, duration, mode, b => b.Acceleration, (b, v) => b.Acceleration = v);

        public static DanmakuEventTemplate AccelerationAngle(GrowthValue amountDeg, GrowthValue start, GrowthValue duration, DanmakuEventMode mode = DanmakuEventMode.Add)
            => Float(amountDeg, start, duration, mode, b => b.AccelerationAngleDeg, (b, v) => b.AccelerationAngleDeg = v);

        public static DanmakuEventTemplate PositionX(GrowthValue amount, GrowthValue start, GrowthValue duration, DanmakuEventMode mode = DanmakuEventMode.Add)
            => Float(amount, start, duration, mode, b => b.GlobalPosition.X, (b, v) => b.GlobalPosition = new Vector2(v, b.GlobalPosition.Y));

        public static DanmakuEventTemplate PositionY(GrowthValue amount, GrowthValue start, GrowthValue duration, DanmakuEventMode mode = DanmakuEventMode.Add)
            => Float(amount, start, duration, mode, b => b.GlobalPosition.Y, (b, v) => b.GlobalPosition = new Vector2(b.GlobalPosition.X, v));

        public static DanmakuEventTemplate ScaleUniform(GrowthValue amount, GrowthValue start, GrowthValue duration, DanmakuEventMode mode = DanmakuEventMode.Add)
            => Float(amount, start, duration, mode, b => b.Scale.X, (b, v) => b.Scale = Vector2.One * v);

        public static DanmakuEventTemplate ScaleX(GrowthValue amount, GrowthValue start, GrowthValue duration, DanmakuEventMode mode = DanmakuEventMode.Add)
            => Float(amount, start, duration, mode, b => b.Scale.X, (b, v) => b.Scale = new Vector2(v, b.Scale.Y));

        public static DanmakuEventTemplate ScaleY(GrowthValue amount, GrowthValue start, GrowthValue duration, DanmakuEventMode mode = DanmakuEventMode.Add)
            => Float(amount, start, duration, mode, b => b.Scale.Y, (b, v) => b.Scale = new Vector2(b.Scale.X, v));

        public static DanmakuEventTemplate MoveForward(GrowthValue distance, GrowthValue start, GrowthValue duration)
            => Directional(distance, start, duration, b => b.AngleRad);

        public static DanmakuEventTemplate MovePerpendicular(GrowthValue distance, GrowthValue start, GrowthValue duration)
            => Directional(distance, start, duration, b => b.AngleRad + Mathf.Pi / 2f);

        public static DanmakuEventTemplate MoveAccAngleForward(GrowthValue distance, GrowthValue start, GrowthValue duration)
            => Directional(distance, start, duration, b => Mathf.DegToRad(b.AccelerationAngleDeg));

        public static DanmakuEventTemplate MoveAccAnglePerpendicular(GrowthValue distance, GrowthValue start, GrowthValue duration)
            => Directional(distance, start, duration, b => Mathf.DegToRad(b.AccelerationAngleDeg) + Mathf.Pi / 2f);

        /// <summary>
        /// Turns the bullet toward its target at `turnSpeedDegPerSec` degrees/second. 
        /// If snapInstantly, jumps to face the target immediately at Start instead.
        /// </summary>
        public static DanmakuEventTemplate Homing(GrowthValue turnSpeedDegPerSec, GrowthValue start, GrowthValue duration, bool snapInstantly = false)
        {
            return new DanmakuEventTemplate
            {
                Resolve = (group, way) =>
                {
                    float turnSpeed = turnSpeedDegPerSec.Evaluate(group, way);
                    return new DanmakuEvent
                    {
                        Start = start.Evaluate(group, way),
                        Duration = Mathf.Max(0.05f, duration.Evaluate(group, way)),
                        Apply = (b, elapsed, t, dt) =>
                        {
                            Vector2? targetPos = b.GetHomingTargetPosition();
                            if (targetPos == null) return;
                            float desiredAngle = (targetPos.Value - b.GlobalPosition).Angle();
                            if (snapInstantly) { b.AngleRad = desiredAngle; return; }
                            float diff = Mathf.Wrap(desiredAngle - b.AngleRad, -Mathf.Pi, Mathf.Pi);
                            float maxStep = Mathf.DegToRad(turnSpeed) * dt;
                            b.AngleRad += Mathf.Clamp(diff, -maxStep, maxStep);
                        },
                    };
                }
            };
        }

        /// <summary>Weaves the bullet side-to-side. Oscillation stops once Duration elapses instead of continuing forever.</summary>
        public static DanmakuEventTemplate SineMovement(GrowthValue amplitude, GrowthValue frequencyHz, GrowthValue start, GrowthValue duration)
        {
            return new DanmakuEventTemplate
            {
                Resolve = (group, way) =>
                {
                    float amp = amplitude.Evaluate(group, way);
                    float freq = frequencyHz.Evaluate(group, way);
                    float lastOffset = 0f;
                    return new DanmakuEvent
                    {
                        Start = start.Evaluate(group, way),
                        Duration = Mathf.Max(0.0001f, duration.Evaluate(group, way)),
                        Apply = (b, elapsed, t, dt) =>
                        {
                            float offset = amp * Mathf.Sin(Mathf.Tau * freq * elapsed);
                            float delta = offset - lastOffset;
                            lastOffset = offset;
                            float perpRad = b.AngleRad + Mathf.Pi / 2f;
                            b.GlobalPosition += new Vector2(Mathf.Cos(perpRad), Mathf.Sin(perpRad)) * delta;
                        },
                    };
                }
            };
        }

        static DanmakuEventTemplate Float(GrowthValue amount, GrowthValue start, GrowthValue duration, DanmakuEventMode mode, Func<NDanmakuBullet, float> get, Action<NDanmakuBullet, float> set)
        {
            return new DanmakuEventTemplate
            {
                Resolve = (group, way) =>
                {
                    float resolvedAmount = amount.Evaluate(group, way);
                    float startValue = 0f;
                    return new DanmakuEvent
                    {
                        Start = start.Evaluate(group, way),
                        Duration = Mathf.Max(0.03f, duration.Evaluate(group, way)),
                        OnStart = b => startValue = get(b),
                        Apply = (b, elapsed, t, dt) =>
                        {
                            float target = mode switch
                            {
                                DanmakuEventMode.Add => startValue + resolvedAmount,
                                DanmakuEventMode.Transition => resolvedAmount,
                                DanmakuEventMode.Multiply => startValue * resolvedAmount,
                                _ => startValue,
                            };
                            set(b, Mathf.Lerp(startValue, target, t));
                        },
                    };
                }
            };
        }

        static DanmakuEventTemplate Directional(GrowthValue totalDistance, GrowthValue start, GrowthValue duration, Func<NDanmakuBullet, float> directionRad)
        {
            return new DanmakuEventTemplate
            {
                Resolve = (group, way) =>
                {
                    float distance = totalDistance.Evaluate(group, way);
                    float lastProgress = 0f;
                    return new DanmakuEvent
                    {
                        Start = start.Evaluate(group, way),
                        Duration = Mathf.Max(0.05f, duration.Evaluate(group, way)),
                        Apply = (b, elapsed, t, dt) =>
                        {
                            float deltaProgress = t - lastProgress;
                            lastProgress = t;
                            float rad = directionRad(b);
                            b.GlobalPosition += new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)) * distance * deltaProgress;
                        },
                    };
                }
            };
        }
    }
}
