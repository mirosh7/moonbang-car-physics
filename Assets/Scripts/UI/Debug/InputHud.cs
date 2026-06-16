using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Debug
{
    /// <summary>Pretty input display: vertical fill bars for throttle / brake /
    /// clutch / handbrake and a steering indicator. Reads InputManager.instance.</summary>
    public class InputHud : MonoBehaviour
    {
        private GameObject m_panel;
        private RectTransform[] m_fills;   // throttle, brake, clutch, handbrake
        private TextMeshProUGUI[] m_values;
        private RectTransform m_steerKnob;

        private static readonly string[] Names = { "Газ", "Тормоз", "Сцепл.", "Ручник" };
        private static readonly Color[] Colors =
        {
            new Color(0.35f, 0.85f, 0.4f),
            new Color(0.95f, 0.35f, 0.3f),
            new Color(0.35f, 0.7f, 1f),
            new Color(1f, 0.65f, 0.25f),
        };

        public static InputHud Create(Transform parent)
        {
            var holder = DebugUI.Container("InputHud", parent);
            DebugUI.Stretch(holder);
            var hud = holder.gameObject.AddComponent<InputHud>();
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
            pr.sizeDelta = new Vector2(620f, 150f);
            pr.anchoredPosition = new Vector2(0f, 200f);

            m_fills = new RectTransform[4];
            m_values = new TextMeshProUGUI[4];

            float x = 18f;
            const float w = 64f, h = 96f;
            for (int i = 0; i < 4; i++)
            {
                var bar = DebugUI.Bar("Bar" + i, panel.transform, new Color(1, 1, 1, 0.10f), Colors[i], out var fill);
                var br = bar.rectTransform;
                br.anchorMin = br.anchorMax = br.pivot = new Vector2(0, 1);
                br.sizeDelta = new Vector2(w, h);
                br.anchoredPosition = new Vector2(x, -10);
                m_fills[i] = fill;

                var name = DebugUI.Text("N" + i, panel.transform, Names[i], 18,
                    TextAlignmentOptions.Center, new Color(1, 1, 1, 0.75f));
                var nr = name.rectTransform;
                nr.anchorMin = nr.anchorMax = nr.pivot = new Vector2(0, 1);
                nr.sizeDelta = new Vector2(w + 12, 24);
                nr.anchoredPosition = new Vector2(x - 6, -10 - h - 2);

                m_values[i] = DebugUI.Text("V" + i, panel.transform, "0", 18,
                    TextAlignmentOptions.Center, Color.white);
                var vr = m_values[i].rectTransform;
                vr.anchorMin = vr.anchorMax = vr.pivot = new Vector2(0, 1);
                vr.sizeDelta = new Vector2(w, 22);
                vr.anchoredPosition = new Vector2(x, -10 - h - 26);

                x += w + 16f;
            }

            // steering indicator (horizontal track + knob), right side
            var title = DebugUI.Text("SteerLbl", panel.transform, "Руль", 18,
                TextAlignmentOptions.Center, new Color(1, 1, 1, 0.75f));
            var tr = title.rectTransform;
            tr.anchorMin = tr.anchorMax = tr.pivot = new Vector2(1, 1);
            tr.sizeDelta = new Vector2(280, 24);
            tr.anchoredPosition = new Vector2(-20, -12);

            var track = DebugUI.Panel("SteerTrack", panel.transform, new Color(1, 1, 1, 0.10f));
            var trr = track.rectTransform;
            trr.anchorMin = trr.anchorMax = trr.pivot = new Vector2(1, 1);
            trr.sizeDelta = new Vector2(280, 26);
            trr.anchoredPosition = new Vector2(-20, -44);

            var center = DebugUI.Panel("SteerCenter", track.transform, new Color(1, 1, 1, 0.3f));
            var cr = center.rectTransform;
            cr.anchorMin = new Vector2(0.5f, 0); cr.anchorMax = new Vector2(0.5f, 1);
            cr.pivot = new Vector2(0.5f, 0.5f); cr.sizeDelta = new Vector2(2, 0); cr.anchoredPosition = Vector2.zero;

            var knob = DebugUI.Panel("SteerKnob", track.transform, new Color(0.4f, 0.8f, 1f));
            m_steerKnob = knob.rectTransform;
            m_steerKnob.anchorMin = m_steerKnob.anchorMax = new Vector2(0.5f, 0.5f);
            m_steerKnob.pivot = new Vector2(0.5f, 0.5f);
            m_steerKnob.sizeDelta = new Vector2(18, 26);
        }

        private void Update()
        {
            var im = InputManager.instance;
            float throttle = im != null ? im.acceleration : 0f;
            float brake = im != null ? im.brakes : 0f;
            float clutch = im != null ? im.clutch : 0f;
            float hand = im != null ? im.handbrake : 0f;
            float steer = im != null ? im.steering : 0f;

            SetFill(0, throttle);
            SetFill(1, brake);
            SetFill(2, clutch);
            SetFill(3, hand);

            if (m_steerKnob != null)
            {
                // map [-1..1] to the track width (track is 280 wide, pivot/anchor centre)
                m_steerKnob.anchoredPosition = new Vector2(Mathf.Clamp(steer, -1f, 1f) * 130f, 0f);
            }
        }

        private void SetFill(int i, float v)
        {
            v = Mathf.Clamp01(v);
            m_fills[i].anchorMin = Vector2.zero;
            m_fills[i].anchorMax = new Vector2(1f, v);
            m_values[i].text = Mathf.RoundToInt(v * 100f).ToString();
        }
    }
}
