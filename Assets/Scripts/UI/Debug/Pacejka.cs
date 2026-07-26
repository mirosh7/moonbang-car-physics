using UnityEngine;
using Car.Data;

namespace UI.Debug
{
    public static class Pacejka
    {
        public static float MagicFormula(float slip, float b, float c, float d, float e)
        {
            float bx = b * slip;
            return d * Mathf.Sin(c * Mathf.Atan(bx - e * (bx - Mathf.Atan(bx))));
        }

        public static float StiffnessFromPeak(float c, float peakSlip)
        {
            if (peakSlip < 1e-6f || c < 1e-6f) return 0f;
            return Mathf.Tan(Mathf.PI / (2f * c)) / peakSlip;
        }

        public static float Fx(CarDesc.WheelInfo w, float slipRatio, float fz)
        {
            float b = StiffnessFromPeak(w.pacejkaShapeLong, w.longSlipPeak);
            float d = w.longitudinalCoeff * Mathf.Max(0f, fz);
            return MagicFormula(slipRatio, b, w.pacejkaShapeLong, d, w.pacejkaCurveLong);
        }

        public static float Fy(CarDesc.WheelInfo w, float slipAngleDeg, float fz)
        {
            float b = StiffnessFromPeak(w.pacejkaShapeLat, w.slipAnglePeak * Mathf.Deg2Rad);
            float d = w.lateralCoeff * Mathf.Max(0f, fz);
            return MagicFormula(slipAngleDeg * Mathf.Deg2Rad, b, w.pacejkaShapeLat, d, w.pacejkaCurveLat);
        }

        public static float PeakFx(CarDesc.WheelInfo w, float fz) => w.longitudinalCoeff * Mathf.Max(0f, fz);

        public static float PeakFy(CarDesc.WheelInfo w, float fz) => w.lateralCoeff * Mathf.Max(0f, fz);
    }
}
