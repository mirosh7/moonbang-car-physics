using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UI.Debug
{
    /// <summary>
    /// Minimal pointer-driven slider built for code-generated UI. Maps the local
    /// pointer X inside its RectTransform to a value in [min..max] and drives a
    /// fill RectTransform. Avoids the finicky anchor setup of UnityEngine.UI.Slider.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class DragSlider : MonoBehaviour, IPointerDownHandler, IDragHandler
    {
        public float min = 0f, max = 1f, value = 0f;
        public RectTransform fill;
        public Action<float> onChanged;

        private RectTransform m_rt;

        private void Awake() => m_rt = (RectTransform)transform;

        public void SetValue(float v, bool notify)
        {
            value = Mathf.Clamp(v, min, max);
            UpdateFill();
            if (notify) onChanged?.Invoke(value);
        }

        private void UpdateFill()
        {
            if (fill == null) return;
            float t = Mathf.InverseLerp(min, max, value);
            fill.anchorMin = new Vector2(0f, 0f);
            fill.anchorMax = new Vector2(t, 1f);
            fill.offsetMin = Vector2.zero;
            fill.offsetMax = Vector2.zero;
        }

        public void OnPointerDown(PointerEventData e) => Apply(e);
        public void OnDrag(PointerEventData e) => Apply(e);

        private void Apply(PointerEventData e)
        {
            if (m_rt == null) m_rt = (RectTransform)transform;
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    m_rt, e.position, e.pressEventCamera, out Vector2 lp))
                return;
            float t = Mathf.InverseLerp(m_rt.rect.xMin, m_rt.rect.xMax, lp.x);
            SetValue(Mathf.Lerp(min, max, t), true);
        }
    }
}
