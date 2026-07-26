using System;
using System.Collections.Generic;
using Car.Data;
using UnityEngine;

namespace UI.Debug
{
    public class TuneParam
    {
        public string category;
        public string label;
        public float min, max;
        public bool isToggle;
        public string[] options;
        public Func<CarDesc, float> get;
        public Action<CarDesc, float> set;
    }

    public static class CarTuning
    {
        public static List<TuneParam> Build()
        {
            var list = new List<TuneParam>();

            AddAxle(list, "Suspension", "Spring stiffness", 10000, 150000,
                w => w.suspensionStiffness, (w, v) => w.suspensionStiffness = v);
            AddAxle(list, "Suspension", "Damper", 0, 8000,
                w => w.damperStiffness, (w, v) => w.damperStiffness = v);
            AddAxle(list, "Suspension", "Rest length", 0.2f, 1.0f,
                w => w.restLength, (w, v) => w.restLength = v);

            AddAxle(list, "Tire", "Long. grip μx", 0.3f, 2.0f,
                w => w.longitudinalCoeff, (w, v) => w.longitudinalCoeff = v);
            AddAxle(list, "Tire", "Lat. grip μy", 0.3f, 2.0f,
                w => w.lateralCoeff, (w, v) => w.lateralCoeff = v);
            AddAxle(list, "Tire", "Peak slip ratio", 0.05f, 0.30f,
                w => w.longSlipPeak, (w, v) => w.longSlipPeak = v);
            AddAxle(list, "Tire", "Peak slip angle, °", 4f, 25f,
                w => w.slipAnglePeak, (w, v) => w.slipAnglePeak = v);

            AddAxle(list, "Alignment", "Camber, °", -6f, 6f,
                w => w.camber, (w, v) => w.camber = v);
            AddAxle(list, "Alignment", "Toe, °", -3f, 3f,
                w => w.toe, (w, v) => w.toe = v);
            AddAxle(list, "Alignment", "Caster, °", 0f, 12f,
                w => w.caster, (w, v) => w.caster = v);
            AddAxle(list, "Alignment", "Camber thrust k", 0f, 1.5f,
                w => w.camberCoeff, (w, v) => w.camberCoeff = v);

            AddAxle(list, "Wheels", "Radius, m", 0.20f, 0.45f,
                w => w.wheelRadius, (w, v) => w.wheelRadius = v);
            AddSingle(list, "Wheels", "Track front, m", 1.0f, 2.0f,
                d => d.trackFront, (d, v) => d.trackFront = v);
            AddSingle(list, "Wheels", "Track rear, m", 1.0f, 2.0f,
                d => d.trackRear, (d, v) => d.trackRear = v);

            AddEnum(list, "Drivetrain", "Drive", new[] { "FWD", "RWD", "AWD" },
                d => (int)d.differentialInfo.driveMode,
                (d, i) => d.differentialInfo.driveMode = (CarDesc.DriveMode)i);
            AddEnum(list, "Drivetrain", "Differential", new[] { "Open", "Locked", "LSD" },
                d => (int)d.differentialInfo.diffType,
                (d, i) => d.differentialInfo.diffType = (CarDesc.DiffType)i);
            AddSingle(list, "Drivetrain", "AWD split (front)", 0f, 1f,
                d => d.differentialInfo.torqueSplitFront, (d, v) => d.differentialInfo.torqueSplitFront = v);
            AddSingle(list, "Drivetrain", "LSD preload", 0f, 300f,
                d => d.differentialInfo.lockingCoeff, (d, v) => d.differentialInfo.lockingCoeff = v);
            AddSingle(list, "Drivetrain", "Final drive", 2.0f, 6.0f,
                d => d.differentialInfo.differentialRatio, (d, v) => d.differentialInfo.differentialRatio = v);

            AddSingle(list, "Chassis", "Brake max, N·m", 500f, 6000f,
                d => d.brakesInfo.maxTorque, (d, v) => d.brakesInfo.maxTorque = v);
            AddToggle(list, "Chassis", "Anti-roll bar",
                d => d.antirollBarInfo.isEnabled, (d, b) => d.antirollBarInfo.isEnabled = b);
            AddSingle(list, "Chassis", "ARB front", 0f, 50000f,
                d => d.antirollBarInfo.stiffnessFront, (d, v) => d.antirollBarInfo.stiffnessFront = v);
            AddSingle(list, "Chassis", "ARB rear", 0f, 50000f,
                d => d.antirollBarInfo.stiffnessRear, (d, v) => d.antirollBarInfo.stiffnessRear = v);

            return list;
        }

        private static void AddAxle(List<TuneParam> list, string cat, string label, float min, float max,
            Func<CarDesc.WheelInfo, float> get, Action<CarDesc.WheelInfo, float> set)
        {
            list.Add(new TuneParam
            {
                category = cat, label = label + " (front)", min = min, max = max,
                get = d => get(d.wheelInfos[0]),
                set = (d, v) => { set(d.wheelInfos[0], v); set(d.wheelInfos[1], v); }
            });
            list.Add(new TuneParam
            {
                category = cat, label = label + " (rear)", min = min, max = max,
                get = d => get(d.wheelInfos[2]),
                set = (d, v) => { set(d.wheelInfos[2], v); set(d.wheelInfos[3], v); }
            });
        }

        private static void AddSingle(List<TuneParam> list, string cat, string label, float min, float max,
            Func<CarDesc, float> get, Action<CarDesc, float> set)
        {
            list.Add(new TuneParam { category = cat, label = label, min = min, max = max, get = get, set = set });
        }

        private static void AddEnum(List<TuneParam> list, string cat, string label, string[] options,
            Func<CarDesc, int> get, Action<CarDesc, int> set)
        {
            list.Add(new TuneParam
            {
                category = cat, label = label, min = 0, max = options.Length - 1, options = options,
                get = d => get(d),
                set = (d, v) => set(d, Mathf.Clamp(Mathf.RoundToInt(v), 0, options.Length - 1))
            });
        }

        private static void AddToggle(List<TuneParam> list, string cat, string label,
            Func<CarDesc, bool> get, Action<CarDesc, bool> set)
        {
            list.Add(new TuneParam
            {
                category = cat, label = label, min = 0, max = 1, isToggle = true,
                get = d => get(d) ? 1f : 0f,
                set = (d, v) => set(d, v >= 0.5f)
            });
        }
    }
}
