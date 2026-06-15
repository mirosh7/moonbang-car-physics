using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Debug
{
    /// <summary>
    /// A scrolling time-series graph with several channels sharing one symmetric
    /// Y axis (auto-scaled). Built in code; feed it one value per channel each
    /// frame via <see cref="Push"/>. Used for the suspension / damper graph.
    /// </summary>
    public class ScrollingGraphPanel : MonoBehaviour
    {
        private const int Capacity = 256;

        private RectTransform m_plot;
        private UILineGraph m_axis;
        private TextMeshProUGUI m_readout;
        private UILineGraph[] m_lines;
        private List<float>[] m_data;
        private readonly List<Vector2> m_buf = new List<Vector2>(Capacity);

        private float m_yMax = 1f;
        private float m_minYMax = 500f;

        public struct Channel { public string name; public Color color; }

        public static ScrollingGraphPanel Create(Transform parent, string title, float minYMax, Channel[] channels)
        {
            var root = DebugUI.Panel("Panel_" + title, parent, new Color(0.05f, 0.05f, 0.07f, 0.85f));
            root.raycastTarget = true;
            var p = root.gameObject.AddComponent<ScrollingGraphPanel>();
            p.m_minYMax = minYMax;
            p.Build(title, channels);
            return p;
        }

        private void Build(string title, Channel[] channels)
        {
            var root = (RectTransform)transform;

            var titleText = DebugUI.Text("Title", root, title, 26, TextAlignmentOptions.TopLeft,
                new Color(0.7f, 1f, 0.8f));
            DebugUI.Place(titleText.rectTransform, new Vector2(14, -8), new Vector2(440, 34));

            m_readout = DebugUI.Text("Readout", root, "", 20, TextAlignmentOptions.TopRight, Color.white);
            var rr = m_readout.rectTransform;
            rr.anchorMin = rr.anchorMax = rr.pivot = new Vector2(1, 1);
            rr.sizeDelta = new Vector2(360, 40);
            rr.anchoredPosition = new Vector2(-14, -8);

            // legend (colored channel names) on its own row under the title
            float lx = 16f;
            foreach (var ch in channels)
            {
                var lt = DebugUI.Text("Leg_" + ch.name, root, "■ " + ch.name, 18,
                    TextAlignmentOptions.TopLeft, ch.color);
                DebugUI.Place(lt.rectTransform, new Vector2(lx, -44), new Vector2(160, 26));
                lx += 160f;
            }

            m_plot = DebugUI.Container("Plot", root);
            m_plot.anchorMin = Vector2.zero;
            m_plot.anchorMax = Vector2.one;
            m_plot.offsetMin = new Vector2(16, 14);
            m_plot.offsetMax = new Vector2(-16, -74);

            var bg = DebugUI.Panel("PlotBg", m_plot, new Color(1, 1, 1, 0.04f));
            DebugUI.Stretch(bg.rectTransform);

            m_axis = DebugUI.Line("AxisZero", m_plot, new Color(1, 1, 1, 0.5f), 2f);
            DebugUI.Stretch(m_axis.rectTransform);
            m_axis.SetPoints(new List<Vector2> { new Vector2(0f, 0.5f), new Vector2(1f, 0.5f) });

            m_lines = new UILineGraph[channels.Length];
            m_data = new List<float>[channels.Length];
            for (int i = 0; i < channels.Length; i++)
            {
                m_lines[i] = DebugUI.Line("Ch_" + channels[i].name, m_plot, channels[i].color, 2.5f);
                DebugUI.Stretch(m_lines[i].rectTransform);
                m_data[i] = new List<float>(Capacity);
            }
        }

        /// <summary>Append one sample per channel (length must match channel count).</summary>
        public void Push(float[] values, string readout)
        {
            if (m_data == null) return;

            float maxAbs = m_minYMax;
            for (int c = 0; c < m_data.Length; c++)
            {
                float v = c < values.Length ? values[c] : 0f;
                var d = m_data[c];
                d.Add(v);
                if (d.Count > Capacity) d.RemoveAt(0);
                for (int i = 0; i < d.Count; i++)
                {
                    float a = Mathf.Abs(d[i]);
                    if (a > maxAbs) maxAbs = a;
                }
            }

            // ease the axis scale so it doesn't jump around
            m_yMax = Mathf.Lerp(m_yMax, maxAbs * 1.1f, 0.15f);
            m_yMax = Mathf.Max(m_minYMax, m_yMax);

            for (int c = 0; c < m_lines.Length; c++)
            {
                var d = m_data[c];
                m_buf.Clear();
                int n = d.Count;
                for (int i = 0; i < n; i++)
                {
                    float x = n > 1 ? i / (float)(Capacity - 1) : 0f;
                    float y = Mathf.Clamp01(Mathf.InverseLerp(-m_yMax, m_yMax, d[i]));
                    m_buf.Add(new Vector2(x, y));
                }
                m_lines[c].SetPoints(m_buf);
            }

            if (m_readout != null) m_readout.text = readout;
        }
    }
}
