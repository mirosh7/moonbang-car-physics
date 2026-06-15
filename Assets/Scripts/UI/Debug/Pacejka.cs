using UnityEngine;
using Car.Data;

namespace UI.Debug
{
    /// <summary>
    /// C# mirror of the native module's Pacejka "Magic Formula" (see
    /// math_util.h). The debug graphs use this to draw exactly the per-axis curve
    /// the DLL integrates, so what the audience sees on the graph is the model,
    /// not an approximation.
    ///
    /// Note: this is the PURE per-axis curve (no combined-slip de-rating). The
    /// live operating point plotted on top comes from the DLL telemetry, which
    /// DOES include the friction-ellipse scaling - so when the tire slips in both
    /// directions at once the dot sits slightly inside the curve. That gap is
    /// itself a nice thing to point at during the demo.
    /// </summary>
    public static class Pacejka
    {
        /// <summary>F(x) = D * sin(C * atan(B*x - E*(B*x - atan(B*x)))).</summary>
        public static float MagicFormula(float slip, float b, float c, float d, float e)
        {
            float bx = b * slip;
            return d * Mathf.Sin(c * Mathf.Atan(bx - e * (bx - Mathf.Atan(bx))));
        }

        /// <summary>Stiffness factor B so the curve peaks at |slip| = peakSlip.</summary>
        public static float StiffnessFromPeak(float c, float peakSlip)
        {
            if (peakSlip < 1e-6f || c < 1e-6f) return 0f;
            return Mathf.Tan(Mathf.PI / (2f * c)) / peakSlip;
        }

        /// <summary>Longitudinal force Fx for a slip ratio at the given normal load Fz (N).</summary>
        public static float Fx(CarDesc.WheelInfo w, float slipRatio, float fz)
        {
            float b = StiffnessFromPeak(w.pacejkaShapeLong, w.longSlipPeak);
            float d = w.longitudinalCoeff * Mathf.Max(0f, fz);
            return MagicFormula(slipRatio, b, w.pacejkaShapeLong, d, w.pacejkaCurveLong);
        }

        /// <summary>Lateral force Fy for a slip angle (degrees) at the given normal load Fz (N).</summary>
        public static float Fy(CarDesc.WheelInfo w, float slipAngleDeg, float fz)
        {
            float b = StiffnessFromPeak(w.pacejkaShapeLat, w.slipAnglePeak * Mathf.Deg2Rad);
            float d = w.lateralCoeff * Mathf.Max(0f, fz);
            return MagicFormula(slipAngleDeg * Mathf.Deg2Rad, b, w.pacejkaShapeLat, d, w.pacejkaCurveLat);
        }

        /// <summary>Peak longitudinal force magnitude (~ D) for axis scaling.</summary>
        public static float PeakFx(CarDesc.WheelInfo w, float fz) => w.longitudinalCoeff * Mathf.Max(0f, fz);
        /// <summary>Peak lateral force magnitude (~ D) for axis scaling.</summary>
        public static float PeakFy(CarDesc.WheelInfo w, float fz) => w.lateralCoeff * Mathf.Max(0f, fz);
    }
}
