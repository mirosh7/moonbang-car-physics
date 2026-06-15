using System.Collections.Generic;
using Car;
using Car.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Debug
{
    /// <summary>
    /// Runtime CarDesc editor. Generates a scrollable list of sliders/toggles from
    /// <see cref="CarTuning"/>; every change writes the CarDesc (symmetrically for
    /// per-axle wheel params) and asks the car to rebuild its native sim live.
    /// </summary>
    public class CarTuneEditor : MonoBehaviour
    {
        private RaceCar m_car;
        private CarDesc m_desc;
        private GameObject m_panel;

        private const float RowH = 54f;
        private const float HeaderH = 46f;

        public static CarTuneEditor Create(Transform parent, RaceCar car)
        {
            var holder = DebugUI.Container("CarTuneEditor", parent);
            DebugUI.Stretch(holder);
            var ed = holder.gameObject.AddComponent<CarTuneEditor>();
            ed.m_car = car;
            ed.m_desc = car != null ? car.desc : null;
            ed.Build(holder);
            return ed;
        }

        public void SetVisible(bool v)
        {
            if (m_panel != null) m_panel.SetActive(v);
        }

        private void Build(RectTransform parent)
        {
            var panel = DebugUI.Panel("TunePanel", parent, new Color(0.05f, 0.05f, 0.07f, 0.92f));
            m_panel = panel.gameObject;
            panel.raycastTarget = true;
            var pr = panel.rectTransform;
            pr.anchorMin = pr.anchorMax = pr.pivot = new Vector2(1f, 1f);
            pr.sizeDelta = new Vector2(640f, 940f);
            pr.anchoredPosition = new Vector2(-16f, -84f);

            var header = DebugUI.Text("Header", panel.transform, "Настройки автомобиля (live)",
                28, TextAlignmentOptions.TopLeft, new Color(0.6f, 0.85f, 1f));
            DebugUI.Place(header.rectTransform, new Vector2(14, -8), new Vector2(600, 36));

            // viewport (clips) + scrollable content
            var viewport = DebugUI.Panel("Viewport", panel.transform, new Color(1, 1, 1, 0.02f));
            viewport.raycastTarget = true;
            var vp = viewport.rectTransform;
            vp.anchorMin = Vector2.zero; vp.anchorMax = Vector2.one;
            vp.offsetMin = new Vector2(8, 8); vp.offsetMax = new Vector2(-8, -52);
            viewport.gameObject.AddComponent<RectMask2D>();

            var content = DebugUI.Container("Content", viewport.transform);
            content.anchorMin = new Vector2(0, 1);
            content.anchorMax = new Vector2(1, 1);
            content.pivot = new Vector2(0, 1);
            content.anchoredPosition = Vector2.zero;

            var scroll = panel.gameObject.AddComponent<ScrollRect>();
            scroll.viewport = vp;
            scroll.content = content;
            scroll.horizontal = false;
            scroll.vertical = true;
            scroll.movementType = ScrollRect.MovementType.Clamped;
            scroll.scrollSensitivity = 28f;

            float y = 0f;
            string lastCat = null;
            var prms = CarTuning.Build();
            foreach (var p in prms)
            {
                if (p.category != lastCat)
                {
                    AddHeader(content, p.category, ref y);
                    lastCat = p.category;
                }
                AddRow(content, p, ref y);
            }
            content.sizeDelta = new Vector2(0, y);
        }

        private void AddHeader(RectTransform content, string text, ref float y)
        {
            var row = DebugUI.Container("Cat_" + text, content);
            Row(row, y, HeaderH);
            var t = DebugUI.Text("H", row, text, 24, TextAlignmentOptions.BottomLeft, new Color(1f, 0.8f, 0.4f));
            DebugUI.Stretch(t.rectTransform);
            t.rectTransform.offsetMin = new Vector2(8, 0);
            y += HeaderH;
        }

        private void AddRow(RectTransform content, TuneParam p, ref float y)
        {
            var row = DebugUI.Container("Row_" + p.label, content);
            Row(row, y, RowH);

            var label = DebugUI.Text("L", row, p.label, 20, TextAlignmentOptions.Left);
            DebugUI.Place(label.rectTransform, new Vector2(10, -8), new Vector2(280, RowH - 12),
                new Vector2(0, 1), new Vector2(0, 1));

            float cur = m_desc != null ? p.get(m_desc) : 0f;
            var valText = DebugUI.Text("V", row, "", 20, TextAlignmentOptions.Right);
            DebugUI.Place(valText.rectTransform, new Vector2(-10, -8), new Vector2(110, RowH - 12),
                new Vector2(1, 1), new Vector2(1, 1));

            if (p.isToggle)
            {
                var btn = DebugUI.Button("T", row, cur >= 0.5f ? "ВКЛ" : "ВЫКЛ", 20, out var btnLabel);
                DebugUI.Place(btn.GetComponent<RectTransform>(), new Vector2(300, -8),
                    new Vector2(150, RowH - 16), new Vector2(0, 1), new Vector2(0, 1));
                valText.text = "";
                btn.onClick.AddListener(() =>
                {
                    float nv = (m_desc != null && p.get(m_desc) >= 0.5f) ? 0f : 1f;
                    Apply(p, nv);
                    btnLabel.text = nv >= 0.5f ? "ВКЛ" : "ВЫКЛ";
                });
            }
            else
            {
                var slider = DebugUI.Slider("S", row, p.min, p.max, cur);
                DebugUI.Place(slider.GetComponent<RectTransform>(), new Vector2(300, -14),
                    new Vector2(250, 26), new Vector2(0, 1), new Vector2(0, 1));
                valText.text = Fmt(cur, p.max);
                slider.onChanged = v =>
                {
                    Apply(p, v);
                    valText.text = Fmt(v, p.max);
                };
            }

            y += RowH;
        }

        private void Apply(TuneParam p, float v)
        {
            if (m_desc == null) return;
            p.set(m_desc, v);
            if (m_car != null) m_car.RequestRebuild();
        }

        private static void Row(RectTransform row, float y, float h)
        {
            row.anchorMin = new Vector2(0, 1);
            row.anchorMax = new Vector2(1, 1);
            row.pivot = new Vector2(0, 1);
            row.sizeDelta = new Vector2(0, h);
            row.anchoredPosition = new Vector2(0, -y);
        }

        private static string Fmt(float v, float max)
        {
            if (max <= 3f) return v.ToString("0.00");
            if (max <= 30f) return v.ToString("0.0");
            return v.ToString("0");
        }
    }
}
