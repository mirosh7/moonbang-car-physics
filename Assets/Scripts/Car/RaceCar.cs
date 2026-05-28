using System;
using System.Collections.Generic;
using Car.Data;
using UnityEngine;

namespace Car
{
    /// <summary>
    /// Engine-side adapter for the native CarPhysics DLL. Owns the simulation
    /// handle, feeds the DLL the data only Unity can provide (raycasts, point
    /// velocities, wheel-root transforms) and applies the forces / visuals the
    /// DLL returns. All physics math lives in the DLL, not here.
    /// </summary>
    public class RaceCar : MonoBehaviour
    {
        public const int WheelCount = CarPhysicsNative.WheelCount;

        private IntPtr m_sim = IntPtr.Zero;
        private Rigidbody m_rb;
        private InputManager m_input;

        private Transform[] m_wheelRoots;
        private Transform[] m_wheelVisuals;
        private Transform[] m_rotationParts;
        private AudioSource[] m_skidSources;

        private float[] m_raycastDistance;
        private int[] m_raycastMask;

        private CarPhysicsNative.WheelInput m_wheelInput;
        private CarPhysicsNative.WheelOutput m_wheelOutput;
        private CarPhysicsNative.DrivetrainOutput m_drivetrainOutput;

        private bool m_gearUpRequested;
        private bool m_gearDownRequested;

        // ----- telemetry exposed to UI / sound -----
        public float engineRpm { get; private set; }
        public float engineMaxRpm { get; private set; }
        public float engineAngularVelocity { get; private set; }
        public float clutchLock { get; private set; }
        public float clutchTorque { get; private set; }
        public int currentGear { get; private set; }
        public float carSpeed => m_rb != null ? m_rb.linearVelocity.magnitude * 3.6f : 0f;

        private float[] m_suspensionForces = new float[WheelCount];
        private float[] m_angularVelocities = new float[WheelCount];
        private float[] m_slipAngles = new float[WheelCount];
        private Vector2[] m_slipForces = new Vector2[WheelCount];
        private Vector3[] m_linearVelocities = new Vector3[WheelCount];

        public IReadOnlyList<float> suspensionForces => m_suspensionForces;
        public IReadOnlyList<float> angularVelocities => m_angularVelocities;
        public IReadOnlyList<float> slipAngles => m_slipAngles;
        public IReadOnlyList<Vector2> slipForces => m_slipForces;
        public IReadOnlyList<Vector3> linearVelocities => m_linearVelocities;

        public void Initialize(CarDesc desc, Rigidbody rb, IList<Transform> wheelRoots,
                               IList<Transform> wheelVisuals, InputManager input)
        {
            m_rb = rb;
            m_input = input;
            engineMaxRpm = desc.engineInfo.engineMaxRpm;

            m_wheelRoots = new Transform[WheelCount];
            m_wheelVisuals = new Transform[WheelCount];
            m_rotationParts = new Transform[WheelCount];
            m_raycastDistance = new float[WheelCount];
            m_raycastMask = new int[WheelCount];
            for (int i = 0; i < WheelCount; i++)
            {
                m_wheelRoots[i] = wheelRoots[i];
                m_wheelVisuals[i] = wheelVisuals[i];
                m_rotationParts[i] = wheelVisuals[i].GetChild(0);
                var w = desc.wheelInfos[i];
                m_raycastDistance[i] = w.restLength + w.wheelRadius;
                m_raycastMask[i] = w.raycastLayer.value;
            }

            float wheelBase = Vector3.Distance(wheelRoots[0].position, wheelRoots[2].position);
            float rearTrack = Vector3.Distance(wheelRoots[2].position, wheelRoots[3].position);

            m_sim = CarPhysicsNative.Create(desc, wheelBase, rearTrack);
            if (m_sim == IntPtr.Zero)
            {
                Debug.LogError("CarPhysics: failed to create native simulation.");
                return;
            }

            m_wheelInput.wheels = new CarPhysicsNative.WheelState[WheelCount];

            SetupSkidAudio();

            if (m_input != null)
            {
                m_input.gearUp += OnGearUp;
                m_input.gearDown += OnGearDown;
            }

           // Debug.Log($"CarPhysics DLL v{CarPhysicsNative.carsim_version()} initialized.");
        }

        private void OnGearUp() => m_gearUpRequested = true;
        private void OnGearDown() => m_gearDownRequested = true;

        private void FixedUpdate()
        {
            if (m_sim == IntPtr.Zero) return;

            float dt = Time.fixedDeltaTime;

            // 1. drivetrain (steering, engine, gearbox, clutch, differential, brakes)
            var dIn = new CarPhysicsNative.DrivetrainInput
            {
                dt = dt,
                throttle = m_input != null ? m_input.acceleration : 0f,
                brake = m_input != null ? m_input.brakes : 0f,
                steer = m_input != null ? m_input.steering : 0f,
                gearUp = m_gearUpRequested ? 1 : 0,
                gearDown = m_gearDownRequested ? 1 : 0,
            };
            m_gearUpRequested = false;
            m_gearDownRequested = false;

            CarPhysicsNative.carsim_update_drivetrain(m_sim, ref dIn, out m_drivetrainOutput);

            // 2. apply steering to wheel roots, neutral-gear body torque
            for (int i = 0; i < WheelCount; i++)
                m_wheelRoots[i].localRotation = Quaternion.Euler(0f, m_drivetrainOutput.steerAngles[i], 0f);
            if (m_drivetrainOutput.applyNeutralTorque != 0)
                m_rb.AddTorque(m_drivetrainOutput.neutralBodyTorque.ToUnity());

            // 3. gather wheel ground contacts (host owns raycasting)
            m_wheelInput.dt = dt;
            for (int i = 0; i < WheelCount; i++)
            {
                Transform root = m_wheelRoots[i];
                var ws = new CarPhysicsNative.WheelState
                {
                    position = CarPhysicsNative.Vec3.From(root.position),
                    right = CarPhysicsNative.Vec3.From(root.right),
                    up = CarPhysicsNative.Vec3.From(root.up),
                    forward = CarPhysicsNative.Vec3.From(root.forward),
                };
                if (Physics.Raycast(root.position, -root.up, out RaycastHit hit,
                                    m_raycastDistance[i], m_raycastMask[i]))
                {
                    ws.hit = 1;
                    ws.hitPoint = CarPhysicsNative.Vec3.From(hit.point);
                    ws.hitNormal = CarPhysicsNative.Vec3.From(hit.normal);
                    ws.pointVelocity = CarPhysicsNative.Vec3.From(m_rb.GetPointVelocity(hit.point));
                }
                m_wheelInput.wheels[i] = ws;
            }

            // 4. wheel forces + visuals (suspension, acceleration, slip, tire)
            CarPhysicsNative.carsim_update_wheels(m_sim, ref m_wheelInput, out m_wheelOutput);

            // 5. apply forces + visuals
            for (int i = 0; i < WheelCount; i++)
            {
                if (m_wheelInput.wheels[i].hit != 0)
                    m_rb.AddForceAtPosition(m_wheelOutput.applyForce[i].ToUnity(),
                                            m_wheelOutput.applyPoint[i].ToUnity());

                m_wheelVisuals[i].position = m_wheelOutput.visualPosition[i].ToUnity();
                m_rotationParts[i].localRotation = Quaternion.Euler(m_wheelOutput.spinEulerX[i], 0f, 0f);
                m_wheelVisuals[i].localRotation = Quaternion.Euler(0f, m_wheelOutput.steerEulerY[i], 0f);

                if (m_skidSources != null && m_skidSources[i] != null)
                    m_skidSources[i].volume = Mathf.Clamp01(Mathf.Abs(m_wheelOutput.normalizedTireMagnitude[i]));
            }

            CacheTelemetry();
        }

        private void CacheTelemetry()
        {
            engineRpm = m_drivetrainOutput.engineRpm;
            engineAngularVelocity = m_drivetrainOutput.engineAngularVelocity;
            clutchLock = m_drivetrainOutput.clutchLock;
            clutchTorque = m_drivetrainOutput.clutchTorque;
            currentGear = m_drivetrainOutput.currentGear;

            for (int i = 0; i < WheelCount; i++)
            {
                m_suspensionForces[i] = m_wheelOutput.suspensionForce[i];
                m_angularVelocities[i] = m_wheelOutput.angularVelocity[i];
                m_slipAngles[i] = m_wheelOutput.slipAngle[i];
                m_slipForces[i] = new Vector2(m_wheelOutput.slipForceLong[i], m_wheelOutput.slipForceLat[i]);
                m_linearVelocities[i] = m_wheelOutput.linearVelocity[i].ToUnity();
            }
        }

        private void SetupSkidAudio()
        {
            var clip = Resources.Load<AudioClip>("Skid");
            var mixer = Resources.Load<UnityEngine.Audio.AudioMixerGroup>("Wheel");
            if (clip == null) return;

            m_skidSources = new AudioSource[WheelCount];
            for (int i = 0; i < WheelCount; i++)
            {
                var src = m_wheelRoots[i].gameObject.AddComponent<AudioSource>();
                src.clip = clip;
                src.loop = true;
                src.volume = 0f;
                if (mixer != null) src.outputAudioMixerGroup = mixer;
                src.Play();
                m_skidSources[i] = src;
            }
        }

        private void OnDestroy()
        {
            if (m_input != null)
            {
                m_input.gearUp -= OnGearUp;
                m_input.gearDown -= OnGearDown;
            }
            if (m_sim != IntPtr.Zero)
            {
                CarPhysicsNative.carsim_destroy(m_sim);
                m_sim = IntPtr.Zero;
            }
        }
    }
}
