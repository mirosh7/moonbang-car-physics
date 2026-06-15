using Car;
using TMPro;
using UnityEngine;

namespace UI.Debug
{
    /// <summary>
    /// Draws the per-wheel tire forces as 3D arrows at the contact point:
    /// longitudinal Fx (blue, along wheel forward), lateral Fy (red, along wheel
    /// right) and vertical Fz (green, along the contact normal), with a numeric
    /// label per wheel. Uses LineRenderer + HDRP/Unlit so it renders under HDRP.
    /// </summary>
    public class WheelForceArrows : MonoBehaviour
    {
        public RaceCar car;
        [Tooltip("Force-to-length scale: how many newtons map to 1 metre of arrow.")]
        public float newtonsPerMeter = 4000f;
        public float shaftWidth = 0.05f;

        private bool m_visible = true;
        private ForceArrow[] m_long, m_lat, m_vert;
        private TextMeshPro[] m_labels;
        private UnityEngine.Camera m_cam;

        public static WheelForceArrows Create(RaceCar car)
        {
            var go = new GameObject("WheelForceArrows");
            var w = go.AddComponent<WheelForceArrows>();
            w.car = car;
            return w;
        }

        public void SetVisible(bool v)
        {
            m_visible = v;
            if (m_long == null) return;
            for (int i = 0; i < m_long.Length; i++)
            {
                m_long[i].SetVisible(v);
                m_lat[i].SetVisible(v);
                m_vert[i].SetVisible(v);
                if (m_labels[i] != null) m_labels[i].enabled = v;
            }
        }

        private void Start()
        {
            var matLong = MakeMat(new Color(0.30f, 0.62f, 1f));
            var matLat = MakeMat(new Color(1f, 0.32f, 0.32f));
            var matVert = MakeMat(new Color(0.40f, 1f, 0.45f));

            int n = RaceCar.WheelCount;
            m_long = new ForceArrow[n];
            m_lat = new ForceArrow[n];
            m_vert = new ForceArrow[n];
            m_labels = new TextMeshPro[n];
            for (int i = 0; i < n; i++)
            {
                m_long[i] = new ForceArrow(transform, matLong, shaftWidth);
                m_lat[i] = new ForceArrow(transform, matLat, shaftWidth);
                m_vert[i] = new ForceArrow(transform, matVert, shaftWidth);
                m_labels[i] = MakeLabel();
            }
            SetVisible(m_visible);
        }

        private void LateUpdate()
        {
            if (car == null || car.telemetry == null) return;
            if (m_cam == null) m_cam = UnityEngine.Camera.main;
            Vector3 camPos = m_cam != null ? m_cam.transform.position : Vector3.zero;
            float s = 1f / Mathf.Max(1f, newtonsPerMeter);

            for (int i = 0; i < RaceCar.WheelCount && i < car.telemetry.Count; i++)
            {
                var t = car.telemetry[i];
                bool show = m_visible && t.grounded;
                m_long[i].SetVisible(show);
                m_lat[i].SetVisible(show);
                m_vert[i].SetVisible(show);
                if (m_labels[i] != null) m_labels[i].enabled = show;
                if (!show) continue;

                Vector3 baseP = t.contactPoint + t.contactNormal * 0.03f;
                m_long[i].Set(baseP, t.wheelForward * (t.fx * s), camPos);
                m_lat[i].Set(baseP, t.wheelRight * (t.fy * s), camPos);
                m_vert[i].Set(baseP, t.wheelUp * (t.fz * s), camPos);

                var lbl = m_labels[i];
                if (lbl != null)
                {
                    lbl.transform.position = baseP + t.contactNormal * 0.55f;
                    if (m_cam != null)
                        lbl.transform.rotation = Quaternion.LookRotation(lbl.transform.position - camPos);
                    lbl.text = $"Fx {t.fx:0}\nFy {t.fy:0}\nFz {t.fz:0}";
                }
            }
        }

        private static TextMeshPro MakeLabel()
        {
            var go = new GameObject("ForceLabel", typeof(RectTransform));
            var tmp = go.AddComponent<TextMeshPro>();
            tmp.fontSize = 3.5f;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;
            tmp.rectTransform.sizeDelta = new Vector2(4f, 3f);
            return tmp;
        }

        private static Material MakeMat(Color c)
        {
            var sh = Shader.Find("HDRP/Unlit");
            if (sh == null) sh = Shader.Find("Unlit/Color");
            var m = new Material(sh);
            if (m.HasProperty("_UnlitColor")) m.SetColor("_UnlitColor", c);
            if (m.HasProperty("_Color")) m.SetColor("_Color", c);
            if (m.HasProperty("_EmissiveColor")) m.SetColor("_EmissiveColor", c * 2f);
            m.EnableKeyword("_EMISSIVE_COLOR");
            m.color = c;
            return m;
        }

        /// <summary>A shaft line + a 2-segment "V" arrowhead, camera-facing.</summary>
        private class ForceArrow
        {
            private readonly LineRenderer m_shaft;
            private readonly LineRenderer m_head;

            public ForceArrow(Transform parent, Material mat, float width)
            {
                m_shaft = NewLine("ArrowShaft", parent, mat, width, 2);
                m_head = NewLine("ArrowHead", parent, mat, width, 3);
            }

            public void SetVisible(bool v)
            {
                m_shaft.enabled = v;
                m_head.enabled = v;
            }

            public void Set(Vector3 basePos, Vector3 vec, Vector3 camPos)
            {
                Vector3 tip = basePos + vec;
                m_shaft.SetPosition(0, basePos);
                m_shaft.SetPosition(1, tip);

                float len = vec.magnitude;
                if (len < 1e-4f) { m_head.enabled = false; return; }
                m_head.enabled = m_shaft.enabled;

                Vector3 dir = vec / len;
                Vector3 toCam = (camPos - tip).normalized;
                Vector3 side = Vector3.Cross(dir, toCam);
                if (side.sqrMagnitude < 1e-6f) side = Vector3.up;
                side.Normalize();

                float h = Mathf.Min(0.25f, len * 0.4f);
                m_head.SetPosition(0, tip - dir * h + side * (h * 0.5f));
                m_head.SetPosition(1, tip);
                m_head.SetPosition(2, tip - dir * h - side * (h * 0.5f));
            }

            private static LineRenderer NewLine(string name, Transform parent, Material mat, float width, int count)
            {
                var go = new GameObject(name);
                go.transform.SetParent(parent, false);
                var lr = go.AddComponent<LineRenderer>();
                lr.useWorldSpace = true;
                lr.material = mat;
                lr.widthMultiplier = width;
                lr.numCapVertices = 2;
                lr.alignment = LineAlignment.View;
                lr.positionCount = count;
                lr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                lr.receiveShadows = false;
                return lr;
            }
        }
    }
}
