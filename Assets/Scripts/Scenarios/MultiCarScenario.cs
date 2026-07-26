using System.Collections.Generic;
using Car;
using Car.Data;
using UnityEngine;

namespace Scenarios
{
    [AddComponentMenu("Car/Scenarios/Multi Car Scenario")]
    public class MultiCarScenario : MonoBehaviour
    {
        public enum Formation { Grid, Line, Circle }
        public enum DriverProfile { FullThrottle, Slalom, CircleRun, Idle }

        [Header("Cars")]
        [SerializeField] private CarDesc m_carDesc;
        [SerializeField, Min(1)] private int m_carCount = 20;
        [SerializeField] private string m_carPrefabName = "porsche";
        [SerializeField] private string m_wheelPrefabName = "porscheWheel";

        [Header("Formation")]
        [SerializeField] private Formation m_formation = Formation.Grid;
        [SerializeField, Min(1)] private int m_carsPerRow = 5;
        [SerializeField] private float m_spacing = 7f;
        [SerializeField] private float m_circleRadius = 30f;
        [SerializeField] private float m_spawnHeight = 0.4f;

        [Header("Driving")]
        [SerializeField] private DriverProfile m_driverProfile = DriverProfile.FullThrottle;
        [SerializeField, Range(0f, 1f)] private float m_throttle = 1f;
        [SerializeField] private float m_slalomPeriod = 4f;
        [SerializeField, Range(0f, 1f)] private float m_slalomSteer = 0.5f;
        [SerializeField, Range(-1f, 1f)] private float m_circleSteer = 0.5f;

        [Header("Auto gearbox")]
        [SerializeField] private float m_shiftUpRpm = 6000f;
        [SerializeField] private float m_shiftDownRpm = 2500f;

        [Header("Misc")]
        [SerializeField] private bool m_muteAudio = true;
        [SerializeField] private bool m_showStats = true;

        private readonly List<Agent> m_agents = new List<Agent>();
        private float m_fps;

        private class Agent
        {
            public RaceCar car;
            public ScriptedCarInput input;
            public float phase;
        }

        public IReadOnlyList<RaceCar> cars
        {
            get
            {
                var list = new List<RaceCar>(m_agents.Count);
                foreach (var a in m_agents) list.Add(a.car);
                return list;
            }
        }

        private void Start()
        {
            var builder = new CarBuilder(m_carDesc, m_carPrefabName, m_wheelPrefabName);
            for (int i = 0; i < m_carCount; i++)
            {
                GetSpawnPose(i, out Vector3 position, out Quaternion rotation);
                var input = new ScriptedCarInput();
                RaceCar car = builder.BuildCar(position, rotation, input);
                car.gameObject.name = $"ScenarioCar_{i:000}";
                if (m_muteAudio) MuteAudio(car.gameObject);
                m_agents.Add(new Agent { car = car, input = input, phase = i * 0.7f });
            }
        }

        private void FixedUpdate()
        {
            float t = Time.time;
            foreach (var a in m_agents)
            {
                Drive(a, t);
                AutoShift(a);
            }
        }

        private void Update()
        {
            m_fps = Mathf.Lerp(m_fps, 1f / Mathf.Max(Time.unscaledDeltaTime, 1e-4f), 0.05f);
        }

        private void Drive(Agent a, float t)
        {
            switch (m_driverProfile)
            {
                case DriverProfile.FullThrottle:
                    a.input.acceleration = m_throttle;
                    a.input.steering = 0f;
                    a.input.brakes = 0f;
                    break;

                case DriverProfile.Slalom:
                    a.input.acceleration = m_throttle;
                    a.input.steering = Mathf.Sin((t + a.phase) * 2f * Mathf.PI / Mathf.Max(m_slalomPeriod, 0.1f)) * m_slalomSteer;
                    a.input.brakes = 0f;
                    break;

                case DriverProfile.CircleRun:
                    a.input.acceleration = m_throttle;
                    a.input.steering = m_circleSteer;
                    a.input.brakes = 0f;
                    break;

                case DriverProfile.Idle:
                    a.input.acceleration = 0f;
                    a.input.steering = 0f;
                    a.input.brakes = 1f;
                    break;
            }
        }

        private void AutoShift(Agent a)
        {
            if (m_driverProfile == DriverProfile.Idle) return;

            int gear = a.car.currentGear;
            int topGear = m_carDesc.gearboxInfo.gearBoxRatios.Count - 1;

            if (gear <= 1)
            {
                a.input.RequestGearUp();
                return;
            }

            if (a.car.engineRpm > m_shiftUpRpm && gear < topGear)
            {
                a.input.RequestGearUp();
            }
            else if (a.car.engineRpm < m_shiftDownRpm && gear > 2)
            {
                a.input.RequestGearDown();
            }
        }

        private void GetSpawnPose(int index, out Vector3 position, out Quaternion rotation)
        {
            Vector3 localPos;
            Quaternion localRot;

            switch (m_formation)
            {
                case Formation.Line:
                {
                    float x = (index - (m_carCount - 1) * 0.5f) * m_spacing;
                    localPos = new Vector3(x, 0f, 0f);
                    localRot = Quaternion.identity;
                    break;
                }
                case Formation.Circle:
                {
                    float angle = index / (float)m_carCount * Mathf.PI * 2f;
                    localPos = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * m_circleRadius;
                    Vector3 tangent = new Vector3(-Mathf.Sin(angle), 0f, Mathf.Cos(angle));
                    localRot = Quaternion.LookRotation(tangent, Vector3.up);
                    break;
                }
                default:
                {
                    int row = index / m_carsPerRow;
                    int col = index % m_carsPerRow;
                    float x = (col - (m_carsPerRow - 1) * 0.5f) * m_spacing;
                    float z = -row * m_spacing;
                    localPos = new Vector3(x, 0f, z);
                    localRot = Quaternion.identity;
                    break;
                }
            }

            position = transform.TransformPoint(localPos + Vector3.up * m_spawnHeight);
            rotation = transform.rotation * localRot;
        }

        private static void MuteAudio(GameObject root)
        {
            foreach (var engineSound in root.GetComponentsInChildren<RealisticEngineSound>(true))
            {
                engineSound.enabled = false;
            }
            foreach (var source in root.GetComponentsInChildren<AudioSource>(true))
            {
                source.Stop();
                source.enabled = false;
            }
        }

        private void OnGUI()
        {
            if (!m_showStats) return;
            GUILayout.BeginArea(new Rect(10, 72, 340, 90), GUI.skin.box);
            GUILayout.Label($"Scenario: {m_driverProfile} / {m_formation}   Cars: {m_agents.Count}");
            GUILayout.Label($"FPS: {m_fps:0}   Fixed dt: {Time.fixedDeltaTime * 1000f:0.0} ms");
            GUILayout.EndArea();
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(0.3f, 0.8f, 1f, 0.8f);
            int preview = Mathf.Min(m_carCount, 200);
            for (int i = 0; i < preview; i++)
            {
                GetSpawnPose(i, out Vector3 p, out Quaternion r);
                Gizmos.DrawWireCube(p + Vector3.up * 0.4f, new Vector3(2f, 0.8f, 4.5f));
                Gizmos.DrawRay(p, r * Vector3.forward * 3f);
            }
        }

        [ContextMenu("Preset: Grid Rush (20 cars)")]
        private void PresetGridRush()
        {
            m_formation = Formation.Grid;
            m_carCount = 20;
            m_carsPerRow = 5;
            m_spacing = 7f;
            m_driverProfile = DriverProfile.FullThrottle;
            m_throttle = 1f;
            m_muteAudio = true;
        }

        [ContextMenu("Preset: Slalom Parade (36 cars)")]
        private void PresetSlalomParade()
        {
            m_formation = Formation.Grid;
            m_carCount = 36;
            m_carsPerRow = 6;
            m_spacing = 9f;
            m_driverProfile = DriverProfile.Slalom;
            m_throttle = 0.7f;
            m_slalomPeriod = 3.5f;
            m_slalomSteer = 0.6f;
            m_muteAudio = true;
        }

        [ContextMenu("Preset: Circle Carnival (12 cars)")]
        private void PresetCircleCarnival()
        {
            m_formation = Formation.Circle;
            m_carCount = 12;
            m_circleRadius = 25f;
            m_driverProfile = DriverProfile.CircleRun;
            m_throttle = 0.8f;
            m_circleSteer = 0.5f;
            m_muteAudio = true;
        }

        [ContextMenu("Preset: Stress Test (100 cars)")]
        private void PresetStressTest()
        {
            m_formation = Formation.Grid;
            m_carCount = 100;
            m_carsPerRow = 10;
            m_spacing = 7f;
            m_driverProfile = DriverProfile.FullThrottle;
            m_throttle = 1f;
            m_muteAudio = true;
        }
    }
}
