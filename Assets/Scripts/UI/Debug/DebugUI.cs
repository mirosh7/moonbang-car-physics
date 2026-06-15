using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Debug
{
    /// <summary>Small factory helpers for building the debug HUD entirely in code,
    /// so the only thing the user has to do in the editor is add one component.</summary>
    public static class DebugUI
    {
        public static RectTransform Container(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return (RectTransform)go.transform;
        }

        public static Image Panel(string name, Transform parent, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.transform.SetParent(parent, false);
            var img = go.GetComponent<Image>();
            img.color = color;
            img.raycastTarget = false;
            return img;
        }

        public static TextMeshProUGUI Text(string name, Transform parent, string text,
            float size, TextAlignmentOptions align, Color? color = null)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var t = go.AddComponent<TextMeshProUGUI>();
            t.text = text;
            t.fontSize = size;
            t.alignment = align;
            t.color = color ?? Color.white;
            t.raycastTarget = false;
            return t;
        }

        public static Button Button(string name, Transform parent, string label, float fontSize,
            out TextMeshProUGUI labelText)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.transform.SetParent(parent, false);
            var img = go.GetComponent<Image>();
            img.color = new Color(0.16f, 0.16f, 0.18f, 0.95f);
            var btn = go.AddComponent<Button>();
            btn.targetGraphic = img;

            labelText = Text(name + "_Label", go.transform, label, fontSize, TextAlignmentOptions.Center);
            Stretch(labelText.rectTransform);
            return btn;
        }

        public static UILineGraph Line(string name, Transform parent, Color color, float thickness)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer));
            go.transform.SetParent(parent, false);
            var line = go.AddComponent<UILineGraph>();
            line.color = color;
            line.thickness = thickness;
            line.raycastTarget = false;
            return line;
        }

        public static DragSlider Slider(string name, Transform parent, float min, float max, float value)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.transform.SetParent(parent, false);
            go.GetComponent<Image>().color = new Color(0.18f, 0.18f, 0.20f, 1f);

            var fill = Panel("Fill", go.transform, new Color(0.30f, 0.55f, 0.90f, 1f));
            fill.rectTransform.anchorMin = Vector2.zero;
            fill.rectTransform.anchorMax = new Vector2(0f, 1f);
            fill.rectTransform.offsetMin = Vector2.zero;
            fill.rectTransform.offsetMax = Vector2.zero;

            var slider = go.AddComponent<DragSlider>();
            slider.min = min;
            slider.max = max;
            slider.fill = fill.rectTransform;
            slider.SetValue(value, false);
            return slider;
        }

        /// <summary>Anchor a RectTransform to fill its parent.</summary>
        public static void Stretch(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        /// <summary>Place a RectTransform with explicit bottom-left anchored layout.</summary>
        public static void Place(RectTransform rt, Vector2 anchoredPos, Vector2 size,
            Vector2? anchor = null, Vector2? pivot = null)
        {
            Vector2 a = anchor ?? new Vector2(0f, 1f);   // default: top-left
            rt.anchorMin = a;
            rt.anchorMax = a;
            rt.pivot = pivot ?? new Vector2(0f, 1f);
            rt.sizeDelta = size;
            rt.anchoredPosition = anchoredPos;
        }
    }
}
