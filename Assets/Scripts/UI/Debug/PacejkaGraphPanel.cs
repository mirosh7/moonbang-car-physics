using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Debug
{
    /// <summary>
    /// A single Pacejka force graph: the theoretical per-axis Magic-Formula curve
    /// (force vs slip) plus a live operating-point dot fed from DLL telemetry.
    /// Built entirely in code; add via <see cref="Create"/>.
    /// </summary>
    public class PacejkaGraphPanel : MonoBehaviour
    {
        private RectTransform m_plot;
        private UILineGraph m_axisX, m_axisY, m_curve;
        private RectTransform m_dot;
        private TextMeshProUGUI m_readout, m_xLabel, m_yLabel;

        private float m_xMin, m_xMax, m_yMax = 1f;
        private readonly List<Vector2> m_curveBuf = new List<Vector2>();
        private const int CurveSamples = 96;

        public static PacejkaGraphPanel Create(Transform parent, string title, Color accent)
        {
            var root = DebugUI.Panel("Panel_" + title, parent, new Color(0.05f, 0.05f, 0.07f, 0.85f));
            root.raycastTarget = true; // panel eats clicks so it doesn't drive the car
            var panel = root.gameObject.AddComponent<PacejkaGraphPanel>();
            panel.Build(title, accent);
            return panel;
        }

        private void Build(string title, Color accent)
        {
            var root = (RectTransform)transform;

            var titleText = DebugUI.Text("Title", root, title, 26, TextAlignmentOptions.TopLeft, accent);
            DebugUI.Place(titleText.rectTransform, new Vector2(14, -8), new Vector2(640, 34));

            m_readout = DebugUI.Text("Readout", root, "", 22, TextAlignmentOptions.TopRight, Color.white);
            var rr = m_readout.rectTransform;
            rr.anchorMin = rr.anchorMax = rr.pivot = new Vector2(1, 1);
            rr.sizeDelta = new Vector2(420, 70);
            rr.anchoredPosition = new Vector2(-14, -8);

            m_plot = DebugUI.Container("Plot", root);
            m_plot.anchorMin = Vector2.zero;
            m_plot.anchorMax = Vector2.one;
            m_plot.offsetMin = new Vector2(54, 40);    // left / bottom padding
            m_plot.offsetMax = new Vector2(-16, -48);  // right / top padding

            var bg = DebugUI.Panel("PlotBg", m_plot, new Color(1, 1, 1, 0.04f));
            DebugUI.Stretch(bg.rectTransform);

            m_axisX = DebugUI.Line("AxisX", m_plot, new Color(1, 1, 1, 0.5f), 2f);
            DebugUI.Stretch(m_axisX.rectTransform);
            m_axisY = DebugUI.Line("AxisY", m_plot, new Color(1, 1, 1, 0.5f), 2f);
            DebugUI.Stretch(m_axisY.rectTransform);

            m_curve = DebugUI.Line("Curve", m_plot, accent, 3.5f);
            DebugUI.Stretch(m_curve.rectTransform);

            var dot = DebugUI.Panel("Dot", m_plot, new Color(1f, 0.9f, 0.2f, 1f));
            m_dot = dot.rectTransform;
            m_dot.anchorMin = m_dot.anchorMax = Vector2.zero;
            m_dot.pivot = new Vector2(0.5f, 0.5f);
            m_dot.sizeDelta = new Vector2(20, 20);

            m_xLabel = DebugUI.Text("XLabel", root, "", 18, TextAlignmentOptions.Bottom, new Color(1, 1, 1, 0.7f));
            var xl = m_xLabel.rectTransform;
            xl.anchorMin = new Vector2(0, 0); xl.anchorMax = new Vector2(1, 0); xl.pivot = new Vector2(0.5f, 0);
            xl.offsetMin = new Vector2(0, 4); xl.offsetMax = new Vector2(0, 26);

            m_yLabel = DebugUI.Text("YLabel", root, "", 18, TextAlignmentOptions.TopLeft, new Color(1, 1, 1, 0.7f));
            DebugUI.Place(m_yLabel.rectTransform, new Vector2(8, -44), new Vector2(220, 24));
        }

        public void SetLabels(string xLabel, string yLabel)
        {
            m_xLabel.text = xLabel;
            m_yLabel.text = yLabel;
        }

        public void SetRanges(float xMin, float xMax, float yMax)
        {
            m_xMin = xMin;
            m_xMax = xMax;
            m_yMax = Mathf.Max(1f, yMax);
            float nx0 = Mathf.Clamp01(Mathf.InverseLerp(m_xMin, m_xMax, 0f));
            m_axisX.SetPoints(new List<Vector2> { new Vector2(0f, 0.5f), new Vector2(1f, 0.5f) });
            m_axisY.SetPoints(new List<Vector2> { new Vector2(nx0, 0f), new Vector2(nx0, 1f) });
        }

        /// <summary>Resample the theoretical curve: x in [xMin..xMax] -> force.</summary>
        public void SetCurve(Func<float, float> force)
        {
            m_curveBuf.Clear();
            for (int i = 0; i < CurveSamples; i++)
            {
                float t = i / (float)(CurveSamples - 1);
                float x = Mathf.Lerp(m_xMin, m_xMax, t);
                m_curveBuf.Add(new Vector2(t, Mathf.InverseLerp(-m_yMax, m_yMax, force(x))));
            }
            m_curve.SetPoints(m_curveBuf);
        }

        /// <summary>Move the live operating-point dot and update the numeric readout.</summary>
        public void SetOperatingPoint(float x, float force, string readout)
        {
            float nx = Mathf.Clamp01(Mathf.InverseLerp(m_xMin, m_xMax, x));
            float ny = Mathf.Clamp01(Mathf.InverseLerp(-m_yMax, m_yMax, force));
            Rect r = m_plot.rect;
            m_dot.anchoredPosition = new Vector2(nx * r.width, ny * r.height);
            m_readout.text = readout;
        }
    }
}
