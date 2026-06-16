using Car;
using TMPro;
using UnityEngine;

namespace UI.Debug
{
    /// <summary>Compact bottom-centre readout: gear, speed (km/h) and engine RPM
    /// with a redline bar. Built in code; toggled from the HUD menu.</summary>
    public class TelemetryHud : MonoBehaviour
    {
        private RaceCar m_car;
        private GameObject m_panel;
        private TextMeshProUGUI m_gear, m_speed, m_speedUnit, m_rpm;
        private RectTransform m_rpmFill;

        public static TelemetryHud Create(Transform parent, RaceCar car)
        {
            var holder = DebugUI.Container("TelemetryHud", parent);
            DebugUI.Stretch(holder);
            var hud = holder.gameObject.AddComponent<TelemetryHud>();
            hud.m_car = car;
            hud.Build(holder);
            return hud;
        }

        public void SetVisible(bool v) { if (m_panel != null) m_panel.SetActive(v); }

        private void Build(RectTransform parent)
        {
            var panel = DebugUI.Panel("Panel", parent, new Color(0.04f, 0.04f, 0.06f, 0.85f));
            m_panel = panel.gameObject;
            var pr = panel.rectTransform;
            pr.anchorMin = pr.anchorMax = pr.pivot = new Vector2(0.5f, 0f);
            pr.sizeDelta = new Vector2(620f, 170f);
            pr.anchoredPosition = new Vector2(0f, 20f);

            // gear (big, centre)
            m_gear = DebugUI.Text("Gear", panel.transform, "N", 96, TextAlignmentOptions.Center,
                new Color(1f, 0.85f, 0.3f));
            Place(m_gear.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(160, 150), new Vector2(0.5f, 0.5f));

            // speed (left)
            m_speed = DebugUI.Text("Speed", panel.transform, "0", 76, TextAlignmentOptions.Right, Color.white);
            Place(m_speed.rectTransform, new Vector2(0.30f, 0.55f), new Vector2(230, 90), new Vector2(0.5f, 0.5f));
            m_speedUnit = DebugUI.Text("Unit", panel.transform, "км/ч", 26, TextAlignmentOptions.Center,
                new Color(1, 1, 1, 0.6f));
            Place(m_speedUnit.rectTransform, new Vector2(0.30f, 0.16f), new Vector2(200, 34), new Vector2(0.5f, 0.5f));

            // rpm number + bar (right)
            m_rpm = DebugUI.Text("Rpm", panel.transform, "0 RPM", 26, TextAlignmentOptions.Center,
                new Color(1, 1, 1, 0.8f));
            Place(m_rpm.rectTransform, new Vector2(0.74f, 0.62f), new Vector2(240, 34), new Vector2(0.5f, 0.5f));

            var bar = DebugUI.Bar("RpmBar", panel.transform, new Color(1, 1, 1, 0.10f),
                new Color(0.95f, 0.4f, 0.25f), out m_rpmFill);
            var br = bar.rectTransform;
            br.anchorMin = br.anchorMax = br.pivot = new Vector2(0.5f, 0.5f);
            br.sizeDelta = new Vector2(240, 22);
            br.anchorMin = br.anchorMax = new Vector2(0.74f, 0.30f);
            br.anchoredPosition = Vector2.zero;
        }

        private void Update()
        {
            if (m_car == null) return;

            int g = m_car.currentGear;
            m_gear.text = g == 0 ? "R" : g == 1 ? "N" : (g - 1).ToString();
            m_speed.text = Mathf.RoundToInt(Mathf.Abs(m_car.carSpeed)).ToString();

            float rpm = m_car.engineRpm;
            float max = Mathf.Max(1000f, m_car.engineMaxRpm);
            m_rpm.text = $"{Mathf.RoundToInt(rpm)} RPM";
            float frac = Mathf.Clamp01(rpm / max);
            m_rpmFill.anchorMin = Vector2.zero;
            m_rpmFill.anchorMax = new Vector2(frac, 1f);
            // tint toward red near the limiter
            float hot = Mathf.InverseLerp(0.8f, 1f, frac);
            m_rpmFill.GetComponent<UnityEngine.UI.Image>().color =
                Color.Lerp(new Color(0.35f, 0.8f, 1f), new Color(1f, 0.2f, 0.15f), hot);
        }

        private static void Place(RectTransform rt, Vector2 anchor, Vector2 size, Vector2 pivot)
        {
            rt.anchorMin = rt.anchorMax = anchor;
            rt.pivot = pivot;
            rt.sizeDelta = size;
            rt.anchoredPosition = Vector2.zero;
        }
    }
}
