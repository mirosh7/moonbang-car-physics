using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Car.Data;
using UnityEngine;

namespace Car
{
    /// <summary>
    /// Thin P/Invoke binding over CarPhysics.dll (the native, engine-agnostic
    /// car-simulation module). Mirrors include/car_physics.h. All the physics
    /// math lives in the DLL; this file only marshals data in and out.
    /// </summary>
    public static class CarPhysicsNative
    {
        private const string DLL = "CarPhysics";
        public const int WheelCount = 4;
        public const float RPM_TO_RAD = Mathf.PI * 2f / 60f;
        public const float RAD_TO_RPM = 1f / RPM_TO_RAD;

        // ----- structs (layout identical to car_physics.h) -----

        [StructLayout(LayoutKind.Sequential)]
        public struct Vec3
        {
            public float x, y, z;
            public Vector3 ToUnity() => new Vector3(x, y, z);
            public static Vec3 From(Vector3 v) => new Vec3 { x = v.x, y = v.y, z = v.z };
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct Curve { public IntPtr times; public IntPtr values; public int count; }

        [StructLayout(LayoutKind.Sequential)]
        private struct EngineInfo
        {
            public Curve torqueCurve;
            public Vec3 engineOrientation;
            public float idleRpm, maxRpm, mul, frictionCoeff, startFriction, inertia;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct GearboxInfo { public IntPtr ratios; public int gearCount; public float shiftTime; }

        [StructLayout(LayoutKind.Sequential)]
        private struct ClutchInfo { public float stiffness, capacity, damping; }

        [StructLayout(LayoutKind.Sequential)]
        private struct DifferentialInfo { public int isLocked; public float ratio; }

        [StructLayout(LayoutKind.Sequential)]
        private struct BrakesInfo { public Curve brakeTorqueCurve; public float maxTorque, biasFront, biasRear; }

        [StructLayout(LayoutKind.Sequential)]
        private struct SteeringInfo { public float turnRadius, steerForce, maxCorrectionAngle, correctionSpeed; }

        [StructLayout(LayoutKind.Sequential)]
        private struct WheelInfo
        {
            public float restLength, suspensionStiffness, damperStiffness, slipAnglePeak,
                         camber, caster, longitudinalCoeff, lateralCoeff, wheelRadius,
                         wheelMass, longFrictionCoeff, relaxationLength;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct CarConfig
        {
            public EngineInfo engine;
            public GearboxInfo gearbox;
            public ClutchInfo clutch;
            public DifferentialInfo differential;
            public BrakesInfo brakes;
            public SteeringInfo steering;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = WheelCount, ArraySubType = UnmanagedType.Struct)]
            public WheelInfo[] wheels;
            public float wheelBase, rearTrack;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct DrivetrainInput
        {
            public float dt, throttle, brake, steer;
            public int gearUp, gearDown;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct DrivetrainOutput
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = WheelCount)]
            public float[] steerAngles;
            public Vec3 neutralBodyTorque;
            public int applyNeutralTorque;
            public float engineRpm, engineAngularVelocity;
            public int currentGear;
            public float clutchTorque, clutchLock;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct WheelState
        {
            public Vec3 position, right, up, forward;
            public int hit;
            public Vec3 hitPoint, hitNormal, pointVelocity;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct WheelInput
        {
            public float dt;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = WheelCount, ArraySubType = UnmanagedType.Struct)]
            public WheelState[] wheels;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct WheelOutput
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = WheelCount, ArraySubType = UnmanagedType.Struct)] public Vec3[] applyForce;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = WheelCount, ArraySubType = UnmanagedType.Struct)] public Vec3[] applyPoint;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = WheelCount, ArraySubType = UnmanagedType.Struct)] public Vec3[] visualPosition;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = WheelCount)] public float[] spinEulerX;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = WheelCount)] public float[] steerEulerY;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = WheelCount)] public float[] angularVelocity;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = WheelCount)] public float[] suspensionForce;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = WheelCount)] public float[] currentLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = WheelCount, ArraySubType = UnmanagedType.Struct)] public Vec3[] linearVelocity;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = WheelCount)] public float[] slipAngle;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = WheelCount)] public float[] lateralAcceleration;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = WheelCount)] public float[] slipForceLong;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = WheelCount)] public float[] slipForceLat;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = WheelCount)] public float[] normalizedTireMagnitude;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = WheelCount)] public float[] fx;
        }

        // ----- exported entry points -----

        [DllImport(DLL, CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr carsim_create(ref CarConfig config);

        [DllImport(DLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void carsim_destroy(IntPtr handle);

        [DllImport(DLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void carsim_update_drivetrain(IntPtr handle, ref DrivetrainInput input, out DrivetrainOutput output);

        [DllImport(DLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void carsim_update_wheels(IntPtr handle, ref WheelInput input, out WheelOutput output);

        [DllImport(DLL, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.LPStr)]
        public static extern string carsim_version();

        // ----- convenience: build a sim from a CarDesc -----

        private const int CurveSamples = 64;

        /// <summary>Creates a native sim from the scriptable CarDesc. Returns the opaque handle.</summary>
        public static IntPtr Create(CarDesc desc, float wheelBase, float rearTrack)
        {
            var pins = new List<GCHandle>();
            try
            {
                var cfg = new CarConfig
                {
                    engine = new EngineInfo
                    {
                        torqueCurve = MakeCurve(desc.engineInfo.torqueCurve, pins),
                        engineOrientation = Vec3.From(desc.engineInfo.engineOrientation),
                        idleRpm = desc.engineInfo.engineIdleRpm,
                        maxRpm = desc.engineInfo.engineMaxRpm,
                        mul = desc.engineInfo.engineMul,
                        frictionCoeff = desc.engineInfo.engineFrictionCoefficient,
                        startFriction = desc.engineInfo.startFriction,
                        inertia = desc.engineInfo.engineInertia,
                    },
                    gearbox = MakeGearbox(desc.gearboxInfo, pins),
                    clutch = new ClutchInfo
                    {
                        stiffness = desc.clutchInfo.clutchStiffness,
                        capacity = desc.clutchInfo.clutchCapacity,
                        damping = desc.clutchInfo.clutchDamping,
                    },
                    differential = new DifferentialInfo
                    {
                        isLocked = desc.differentialInfo.isDiffLocked ? 1 : 0,
                        ratio = desc.differentialInfo.differentialRatio,
                    },
                    brakes = new BrakesInfo
                    {
                        brakeTorqueCurve = MakeCurve(desc.brakesInfo.brakeTorqueCurve, pins),
                        maxTorque = desc.brakesInfo.maxTorque,
                        biasFront = desc.brakesInfo.brakeBias[0],
                        biasRear = desc.brakesInfo.brakeBias[1],
                    },
                    steering = new SteeringInfo
                    {
                        turnRadius = desc.steeringInfo.turnRadius,
                        steerForce = desc.steeringInfo.steerForce,
                        maxCorrectionAngle = desc.steeringInfo.maxCorrectionAngle,
                        correctionSpeed = desc.steeringInfo.correctionSpeed,
                    },
                    wheels = new WheelInfo[WheelCount],
                    wheelBase = wheelBase,
                    rearTrack = rearTrack,
                };

                for (int i = 0; i < WheelCount; i++)
                {
                    var w = desc.wheelInfos[i];
                    cfg.wheels[i] = new WheelInfo
                    {
                        restLength = w.restLength,
                        suspensionStiffness = w.suspensionStiffness,
                        damperStiffness = w.damperStiffness,
                        slipAnglePeak = w.slipAnglePeak,
                        camber = w.camber,
                        caster = w.caster,
                        longitudinalCoeff = w.longitudinalCoeff,
                        lateralCoeff = w.lateralCoeff,
                        wheelRadius = w.wheelRadius,
                        wheelMass = w.wheelInertia / (w.wheelRadius * w.wheelRadius), // recover mass
                        longFrictionCoeff = w.longFrictionCoeff,
                        relaxationLength = w.relaxationLength,
                    };
                }

                return carsim_create(ref cfg);
            }
            finally
            {
                foreach (var h in pins) h.Free();
            }
        }

        private static Curve MakeCurve(AnimationCurve curve, List<GCHandle> pins)
        {
            // Sample Unity's (Hermite) curve into a dense piecewise-linear table
            // so the DLL's linear evaluation closely matches the editor curve.
            float[] times, values;
            if (curve == null || curve.length == 0)
            {
                times = new float[] { 0f };
                values = new float[] { 0f };
            }
            else
            {
                float t0 = curve.keys[0].time;
                float t1 = curve.keys[curve.length - 1].time;
                int n = curve.length == 1 ? 1 : CurveSamples;
                times = new float[n];
                values = new float[n];
                for (int i = 0; i < n; i++)
                {
                    float t = n == 1 ? t0 : Mathf.Lerp(t0, t1, i / (float)(n - 1));
                    times[i] = t;
                    values[i] = curve.Evaluate(t);
                }
            }
            return new Curve { times = Pin(times, pins), values = Pin(values, pins), count = times.Length };
        }

        private static GearboxInfo MakeGearbox(CarDesc.GearboxInfo info, List<GCHandle> pins)
        {
            var ratios = info.gearBoxRatios.ToArray();
            return new GearboxInfo { ratios = Pin(ratios, pins), gearCount = ratios.Length, shiftTime = info.shiftTime };
        }

        private static IntPtr Pin(float[] data, List<GCHandle> pins)
        {
            var h = GCHandle.Alloc(data, GCHandleType.Pinned);
            pins.Add(h);
            return h.AddrOfPinnedObject();
        }
    }
}
