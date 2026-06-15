using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Debug
{
    /// <summary>
    /// Lightweight uGUI polyline. Points are given in normalized [0..1] coordinates
    /// inside the RectTransform (0,0 = bottom-left, 1,1 = top-right). It builds a
    /// triangle mesh in a Canvas Graphic, so it renders in ANY render pipeline
    /// (Built-in / URP / HDRP) - unlike GL immediate-mode overlays, which need a
    /// custom pass under HDRP.
    /// </summary>
    [RequireComponent(typeof(CanvasRenderer))]
    public class UILineGraph : MaskableGraphic
    {
        [Range(0.5f, 12f)] public float thickness = 2.5f;
        public bool closed = false;

        private readonly List<Vector2> m_points = new List<Vector2>();

        public void SetPoints(IReadOnlyList<Vector2> normalizedPoints)
        {
            m_points.Clear();
            if (normalizedPoints != null)
                for (int i = 0; i < normalizedPoints.Count; i++) m_points.Add(normalizedPoints[i]);
            SetVerticesDirty();
        }

        public void Clear()
        {
            m_points.Clear();
            SetVerticesDirty();
        }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            if (m_points.Count < 2) return;

            Rect r = rectTransform.rect;
            int segs = closed ? m_points.Count : m_points.Count - 1;
            for (int i = 0; i < segs; i++)
            {
                Vector2 a = ToLocal(m_points[i], r);
                Vector2 b = ToLocal(m_points[(i + 1) % m_points.Count], r);
                AddSegment(vh, a, b);
            }
        }

        private static Vector2 ToLocal(Vector2 n, Rect r)
            => new Vector2(r.xMin + n.x * r.width, r.yMin + n.y * r.height);

        private void AddSegment(VertexHelper vh, Vector2 a, Vector2 b)
        {
            Vector2 dir = b - a;
            float len = dir.magnitude;
            if (len < 1e-4f) return;
            dir /= len;
            Vector2 nrm = new Vector2(-dir.y, dir.x) * (thickness * 0.5f);

            int idx = vh.currentVertCount;
            UIVertex v = UIVertex.simpleVert;
            v.color = color;
            v.position = a - nrm; vh.AddVert(v);
            v.position = a + nrm; vh.AddVert(v);
            v.position = b + nrm; vh.AddVert(v);
            v.position = b - nrm; vh.AddVert(v);
            vh.AddTriangle(idx, idx + 1, idx + 2);
            vh.AddTriangle(idx + 2, idx + 3, idx);
        }
    }
}
