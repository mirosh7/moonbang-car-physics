using Car;
using Car.Data;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI.Debug
{
    /// <summary>
    /// Self-contained debug HUD for presenting the car-physics module. Builds its
    /// whole UI in code (Screen-Space Canvas) so the only editor step is: add this
    /// component to a GameObject and press Play. Renders the Pacejka longitudinal
    /// (Fx vs slip ratio) and lateral (Fy vs slip angle) force curves with a live
    /// operating point, a top toggle menu and a per-wheel selector.
    ///
    /// Works under HDRP (no GL / OnPostRender): everything is uGUI mesh.
    /// </summary>
    [AddComponentMenu("Car/Debug/Car Debug HUD")]
    public class CarDebugHud : MonoBehaviour
    {
        [Tooltip("Leave empty to auto-find the RaceCar in the scene.")]
        [SerializeField] private RaceCar m_car;

        [Header("Toggles (also on the top menu)")]
        [SerializeField] private bool m_showFx = true;
        [SerializeField] private bool m_showFy = true;
        [SerializeField] private bool m_showArrows = true;
        [SerializeField] private bool m_showEditor = false;
        [SerializeField] private bool m_showSuspension = true;

        private static readonly string[] WheelNames = { "FL", "FR", "RL", "RR" };
        private static readonly Color Accent = new Color(0.30f, 0.75f, 1f);
        private static readonly Color AccentLat = new Color(1f, 0.55f, 0.35f);
        private static readonly Color BtnOn = new Color(0.20f, 0.50f, 0.85f, 0.95f);
        private static readonly Color BtnOff = new Color(0.16f, 0.16f, 0.18f, 0.95f);

        private int m_selected = 0;
        private Canvas m_canvas;
        private PacejkaGraphPanel m_panelFx, m_panelFy;
        private ScrollingGraphPanel m_panelSusp;
        private WheelForceArrows m_arrows;
        private CarTuneEditor m_editor;
        private readonly Button[] m_wheelButtons = new Button[4];
        private Button m_btnFx, m_btnFy, m_btnArrows, m_btnEditor, m_btnSusp;

        private void Start()
        {
            if (m_car == null) m_car = FindAnyObjectByType<RaceCar>();
            EnsureEventSystem();
            BuildUI();
            RefreshButtonTints();
        }

        private void Update()
        {
            // keyboard fallbacks (old input backend is enabled in this project)
            if (Input.GetKeyDown(KeyCode.Alpha1)) SelectWheel(0);
            if (Input.GetKeyDown(KeyCode.Alpha2)) SelectWheel(1);
            if (Input.GetKeyDown(KeyCode.Alpha3)) SelectWheel(2);
            if (Input.GetKeyDown(KeyCode.Alpha4)) SelectWheel(3);
            if (Input.GetKeyDown(KeyCode.F1) && m_canvas != null)
                m_canvas.enabled = !m_canvas.enabled;

            RefreshGraphs();
        }

        // ------------------------------------------------------------------ UI
        private void BuildUI()
        {
            var canvasGo = new GameObject("CarDebugHud_Canvas",
                typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasGo.transform.SetParent(transform, false);
            m_canvas = canvasGo.GetComponent<Canvas>();
            m_canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            m_canvas.sortingOrder = 1000;
            var scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
            var root = (RectTransform)canvasGo.transform;

            BuildMenu(root);

            m_panelFx = PacejkaGraphPanel.Create(root, "Pacejka  Fx (продольная сила)", Accent);
            PlaceBottomLeft((RectTransform)m_panelFx.transform, new Vector2(20, 20), new Vector2(780, 440));
            m_panelFx.SetLabels("slip ratio  κ", "Fx, Н");

            m_panelFy = PacejkaGraphPanel.Create(root, "Pacejka  Fy (поперечная сила)", AccentLat);
            PlaceBottomRight((RectTransform)m_panelFy.transform, new Vector2(-20, 20), new Vector2(780, 440));
            m_panelFy.SetLabels("slip angle  α, град", "Fy, Н");

            m_panelSusp = ScrollingGraphPanel.Create(root, "Подвеска / амортизатор (Н)", 2000f,
                new[]
                {
                    new ScrollingGraphPanel.Channel { name = "Пружина", color = new Color(0.4f, 0.8f, 1f) },
                    new ScrollingGraphPanel.Channel { name = "Демпфер", color = new Color(1f, 0.65f, 0.2f) },
                    new ScrollingGraphPanel.Channel { name = "Σ Fz",    color = Color.white },
                });
            PlaceTopLeft((RectTransform)m_panelSusp.transform, new Vector2(20, -84), new Vector2(820, 360));

            m_panelFx.gameObject.SetActive(m_showFx);
            m_panelFy.gameObject.SetActive(m_showFy);
            m_panelSusp.gameObject.SetActive(m_showSuspension);

            if (m_car != null)
            {
                m_arrows = WheelForceArrows.Create(m_car);
                m_arrows.SetVisible(m_showArrows);
                m_editor = CarTuneEditor.Create(root, m_car);
                m_editor.SetVisible(m_showEditor);
            }
        }

        private void BuildMenu(RectTransform root)
        {
            var bar = DebugUI.Panel("MenuBar", root, new Color(0.04f, 0.04f, 0.06f, 0.9f));
            var rt = bar.rectTransform;
            rt.anchorMin = new Vector2(0, 1);
            rt.anchorMax = new Vector2(1, 1);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.sizeDelta = new Vector2(0, 72);
            rt.anchoredPosition = Vector2.zero;
            bar.raycastTarget = true;

            float x = 16f;
            var wheelLbl = DebugUI.Text("Lbl", bar.transform, "Колесо:", 24, TextAlignmentOptions.Left);
            DebugUI.Place(wheelLbl.rectTransform, new Vector2(x, -12), new Vector2(120, 48));
            x += 130f;

            for (int i = 0; i < 4; i++)
            {
                int idx = i;
                var btn = DebugUI.Button("W" + WheelNames[i], bar.transform, WheelNames[i], 24, out _);
                DebugUI.Place(btn.GetComponent<RectTransform>(), new Vector2(x, -12), new Vector2(72, 48));
                btn.onClick.AddListener(() => SelectWheel(idx));
                m_wheelButtons[i] = btn;
                x += 80f;
            }

            x += 24f;
            m_btnFx = DebugUI.Button("ToggleFx", bar.transform, "График Fx", 24, out _);
            DebugUI.Place(m_btnFx.GetComponent<RectTransform>(), new Vector2(x, -12), new Vector2(180, 48));
            m_btnFx.onClick.AddListener(() => { m_showFx = !m_showFx; m_panelFx.gameObject.SetActive(m_showFx); RefreshButtonTints(); });
            x += 192f;

            m_btnFy = DebugUI.Button("ToggleFy", bar.transform, "График Fy", 24, out _);
            DebugUI.Place(m_btnFy.GetComponent<RectTransform>(), new Vector2(x, -12), new Vector2(180, 48));
            m_btnFy.onClick.AddListener(() => { m_showFy = !m_showFy; m_panelFy.gameObject.SetActive(m_showFy); RefreshButtonTints(); });
            x += 192f;

            m_btnArrows = DebugUI.Button("ToggleArrows", bar.transform, "3D Силы", 24, out _);
            DebugUI.Place(m_btnArrows.GetComponent<RectTransform>(), new Vector2(x, -12), new Vector2(170, 48));
            m_btnArrows.onClick.AddListener(() => { m_showArrows = !m_showArrows; if (m_arrows != null) m_arrows.SetVisible(m_showArrows); RefreshButtonTints(); });
            x += 182f;

            m_btnSusp = DebugUI.Button("ToggleSusp", bar.transform, "Подвеска", 24, out _);
            DebugUI.Place(m_btnSusp.GetComponent<RectTransform>(), new Vector2(x, -12), new Vector2(180, 48));
            m_btnSusp.onClick.AddListener(() => { m_showSuspension = !m_showSuspension; if (m_panelSusp != null) m_panelSusp.gameObject.SetActive(m_showSuspension); RefreshButtonTints(); });
            x += 192f;

            m_btnEditor = DebugUI.Button("ToggleEditor", bar.transform, "Настройки", 24, out _);
            DebugUI.Place(m_btnEditor.GetComponent<RectTransform>(), new Vector2(x, -12), new Vector2(190, 48));
            m_btnEditor.onClick.AddListener(() => { m_showEditor = !m_showEditor; if (m_editor != null) m_editor.SetVisible(m_showEditor); RefreshButtonTints(); });
            x += 202f;

            var hint = DebugUI.Text("Hint", bar.transform, "1-4: колесо   F1: скрыть", 20,
                TextAlignmentOptions.Left, new Color(1, 1, 1, 0.5f));
            DebugUI.Place(hint.rectTransform, new Vector2(x, -16), new Vector2(360, 40));
        }

        // -------------------------------------------------------------- refresh
        private void RefreshGraphs()
        {
            if (m_car == null || m_car.desc == null) return;
            if (m_car.telemetry == null || m_car.telemetry.Count <= m_selected) return;

            var w = m_car.desc.wheelInfos[m_selected];
            var t = m_car.telemetry[m_selected];
            float fz = Mathf.Max(1f, t.fz);

            if (m_showFx)
            {
                float yMax = 1.15f * Pacejka.PeakFx(w, fz);
                m_panelFx.SetRanges(-1f, 1f, yMax);
                m_panelFx.SetCurve(x => Pacejka.Fx(w, x, fz));
                m_panelFx.SetOperatingPoint(t.slipRatio, t.fx,
                    $"κ = {t.slipRatio:0.000}\nFx = {t.fx:0} Н\nFz = {fz:0} Н");
            }

            if (m_showFy)
            {
                float yMax = 1.15f * Pacejka.PeakFy(w, fz);
                m_panelFy.SetRanges(-20f, 20f, yMax);
                m_panelFy.SetCurve(a => Pacejka.Fy(w, a, fz));
                m_panelFy.SetOperatingPoint(t.slipAngleDeg, t.fy,
                    $"α = {t.slipAngleDeg:0.0}°\nFy = {t.fy:0} Н\nμ-исп. = {t.normalizedMag * 100f:0}%");
            }

            if (m_showSuspension && m_panelSusp != null)
            {
                // Spring and damper contributions, computed the same way the DLL does
                // (Fz telemetry is their clamped sum). Travel = compression from rest.
                float travel = w.restLength - t.suspensionLength;
                float spring = travel * w.suspensionStiffness;
                float damper = t.suspensionVel * w.damperStiffness;
                m_panelSusp.Push(new[] { spring, damper, t.fz },
                    $"ход {travel * 1000f:0} мм\nv {t.suspensionVel:0.00} м/с");
            }
        }

        private void SelectWheel(int idx)
        {
            m_selected = Mathf.Clamp(idx, 0, 3);
            RefreshButtonTints();
        }

        private void RefreshButtonTints()
        {
            for (int i = 0; i < 4; i++)
                if (m_wheelButtons[i] != null)
                    m_wheelButtons[i].targetGraphic.GetComponent<Image>().color = (i == m_selected) ? BtnOn : BtnOff;
            if (m_btnFx != null) m_btnFx.targetGraphic.GetComponent<Image>().color = m_showFx ? BtnOn : BtnOff;
            if (m_btnFy != null) m_btnFy.targetGraphic.GetComponent<Image>().color = m_showFy ? BtnOn : BtnOff;
            if (m_btnArrows != null) m_btnArrows.targetGraphic.GetComponent<Image>().color = m_showArrows ? BtnOn : BtnOff;
            if (m_btnSusp != null) m_btnSusp.targetGraphic.GetComponent<Image>().color = m_showSuspension ? BtnOn : BtnOff;
            if (m_btnEditor != null) m_btnEditor.targetGraphic.GetComponent<Image>().color = m_showEditor ? BtnOn : BtnOff;
        }

        // ------------------------------------------------------------- helpers
        private static void EnsureEventSystem()
        {
            if (EventSystem.current != null) return;
            if (FindObjectOfType<EventSystem>() != null) return;
            var go = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
            DontDestroyOnLoad(go);
        }

        private static void PlaceTopLeft(RectTransform rt, Vector2 pos, Vector2 size)
        {
            rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0, 1);
            rt.sizeDelta = size;
            rt.anchoredPosition = pos;
        }

        private static void PlaceBottomLeft(RectTransform rt, Vector2 pos, Vector2 size)
        {
            rt.anchorMin = rt.anchorMax = rt.pivot = Vector2.zero;
            rt.sizeDelta = size;
            rt.anchoredPosition = pos;
        }

        private static void PlaceBottomRight(RectTransform rt, Vector2 pos, Vector2 size)
        {
            rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(1, 0);
            rt.sizeDelta = size;
            rt.anchoredPosition = pos;
        }
    }
}
