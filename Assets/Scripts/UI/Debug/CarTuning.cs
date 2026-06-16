using System;
using System.Collections.Generic;
using Car.Data;
using UnityEngine;

namespace UI.Debug
{
    /// <summary>One editable car parameter. Wheel parameters are grouped per axle
    /// and the setter writes BOTH wheels of the axle, so a single control tunes the
    /// left and right symmetrically (exactly what the editor exposes).</summary>
    public class TuneParam
    {
        public string category;
        public string label;
        public float min, max;
        public bool isToggle;
        public string[] options;          // non-null => enum cycle control (get/set is the index)
        public Func<CarDesc, float> get;
        public Action<CarDesc, float> set;
    }

    public static class CarTuning
    {
        public static List<TuneParam> Build()
        {
            var list = new List<TuneParam>();

            // --- Подвеска ---
            AddAxle(list, "Подвеска", "Жёсткость пружины", 10000, 150000,
                w => w.suspensionStiffness, (w, v) => w.suspensionStiffness = v);
            AddAxle(list, "Подвеска", "Демпфер", 0, 8000,
                w => w.damperStiffness, (w, v) => w.damperStiffness = v);
            AddAxle(list, "Подвеска", "Длина покоя", 0.2f, 1.0f,
                w => w.restLength, (w, v) => w.restLength = v);

            // --- Шина (Pacejka) ---
            AddAxle(list, "Шина", "Сцепление прод. μx", 0.3f, 2.0f,
                w => w.longitudinalCoeff, (w, v) => w.longitudinalCoeff = v);
            AddAxle(list, "Шина", "Сцепление попер. μy", 0.3f, 2.0f,
                w => w.lateralCoeff, (w, v) => w.lateralCoeff = v);
            AddAxle(list, "Шина", "Пик slip ratio", 0.05f, 0.30f,
                w => w.longSlipPeak, (w, v) => w.longSlipPeak = v);
            AddAxle(list, "Шина", "Пик slip angle, °", 4f, 25f,
                w => w.slipAnglePeak, (w, v) => w.slipAnglePeak = v);

            // --- Развал-схождение ---
            AddAxle(list, "Развал-схождение", "Развал, °", -6f, 6f,
                w => w.camber, (w, v) => w.camber = v);
            AddAxle(list, "Развал-схождение", "Схождение, °", -3f, 3f,
                w => w.toe, (w, v) => w.toe = v);
            AddAxle(list, "Развал-схождение", "Кастер, °", 0f, 12f,
                w => w.caster, (w, v) => w.caster = v);
            AddAxle(list, "Развал-схождение", "Camber thrust k", 0f, 1.5f,
                w => w.camberCoeff, (w, v) => w.camberCoeff = v);

            // --- Колёса / геометрия ---
            AddAxle(list, "Колёса", "Радиус, м", 0.20f, 0.45f,
                w => w.wheelRadius, (w, v) => w.wheelRadius = v);
            AddSingle(list, "Колёса", "Колея перёд, м", 1.0f, 2.0f,
                d => d.trackFront, (d, v) => d.trackFront = v);
            AddSingle(list, "Колёса", "Колея зад, м", 1.0f, 2.0f,
                d => d.trackRear, (d, v) => d.trackRear = v);

            // --- Трансмиссия ---
            AddEnum(list, "Трансмиссия", "Привод", new[] { "FWD", "RWD", "AWD" },
                d => (int)d.differentialInfo.driveMode,
                (d, i) => d.differentialInfo.driveMode = (CarDesc.DriveMode)i);
            AddEnum(list, "Трансмиссия", "Дифференциал", new[] { "Открытый", "Заблок.", "LSD" },
                d => (int)d.differentialInfo.diffType,
                (d, i) => d.differentialInfo.diffType = (CarDesc.DiffType)i);
            AddSingle(list, "Трансмиссия", "AWD split (перёд)", 0f, 1f,
                d => d.differentialInfo.torqueSplitFront, (d, v) => d.differentialInfo.torqueSplitFront = v);
            AddSingle(list, "Трансмиссия", "LSD преднатяг", 0f, 300f,
                d => d.differentialInfo.lockingCoeff, (d, v) => d.differentialInfo.lockingCoeff = v);
            AddSingle(list, "Трансмиссия", "Главная пара", 2.0f, 6.0f,
                d => d.differentialInfo.differentialRatio, (d, v) => d.differentialInfo.differentialRatio = v);

            // --- Шасси ---
            AddSingle(list, "Шасси", "Тормоз макс, Н·м", 500f, 6000f,
                d => d.brakesInfo.maxTorque, (d, v) => d.brakesInfo.maxTorque = v);
            AddToggle(list, "Шасси", "Стабилизатор вкл",
                d => d.antirollBarInfo.isEnabled, (d, b) => d.antirollBarInfo.isEnabled = b);
            AddSingle(list, "Шасси", "Стаб. перёд", 0f, 50000f,
                d => d.antirollBarInfo.stiffnessFront, (d, v) => d.antirollBarInfo.stiffnessFront = v);
            AddSingle(list, "Шасси", "Стаб. зад", 0f, 50000f,
                d => d.antirollBarInfo.stiffnessRear, (d, v) => d.antirollBarInfo.stiffnessRear = v);

            return list;
        }

        private static void AddAxle(List<TuneParam> list, string cat, string label, float min, float max,
            Func<CarDesc.WheelInfo, float> get, Action<CarDesc.WheelInfo, float> set)
        {
            list.Add(new TuneParam
            {
                category = cat, label = label + " (перёд)", min = min, max = max,
                get = d => get(d.wheelInfos[0]),
                set = (d, v) => { set(d.wheelInfos[0], v); set(d.wheelInfos[1], v); }
            });
            list.Add(new TuneParam
            {
                category = cat, label = label + " (зад)", min = min, max = max,
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
