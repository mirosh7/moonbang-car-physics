using System;
using System.Collections.Generic;
using Car.Data;
using Unity.Profiling;
using UnityEngine;

namespace Car
{
	public class RaceCar : MonoBehaviour
	{
		public const int WheelCount = CarPhysicsNative.WheelCount;

		private IntPtr m_sim = IntPtr.Zero;
		private Rigidbody m_rb;
		private ICarInput m_input;

		private Transform[] m_wheelRoots;
		private Transform[] m_wheelVisuals;
		private Transform[] m_rotationParts;
		private AudioSource[] m_skidSources;

		private float[] m_raycastDistance;
		private int[] m_raycastMask;

		private readonly float[] m_wheelToe = new float[WheelCount];
		private readonly float[] m_wheelCamber = new float[WheelCount];
		private readonly float[] m_wheelCaster = new float[WheelCount];

		private Vector3[] m_baseWheelScale;
		private float[] m_baseRadius;
		private bool m_rebuildRequested;

		private CarPhysicsNative.WheelInput m_wheelInput;
		private CarPhysicsNative.WheelOutput m_wheelOutput;
		private CarPhysicsNative.DrivetrainOutput m_drivetrainOutput;

		private bool m_gearUpRequested;
		private bool m_gearDownRequested;

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

		public struct WheelTelemetry
		{
			public float slipRatio;
			public float slipAngleDeg;
			public float fx;
			public float fy;
			public float fz;
			public float normalizedMag;
			public float suspensionLength;
			public float suspensionVel;
			public float angularVelocity;
			public bool  grounded;
			public Vector3 contactPoint;
			public Vector3 contactNormal;
			public Vector3 wheelForward;
			public Vector3 wheelRight;
			public Vector3 wheelUp;
		}

		private WheelTelemetry[] m_telemetry = new WheelTelemetry[WheelCount];
		private float[] m_lastSuspensionLength = new float[WheelCount];
		private CarDesc m_desc;

		public IReadOnlyList<WheelTelemetry> telemetry => m_telemetry;
		public CarDesc desc => m_desc;

		public void Initialize(CarDesc desc, Rigidbody rb, IList<Transform> wheelRoots,
			IList<Transform> wheelVisuals, ICarInput input)
		{
			m_rb = rb;
			m_input = input;
			m_desc = desc;
			engineMaxRpm = desc.engineInfo.engineMaxRpm;

			m_wheelRoots = new Transform[WheelCount];
			m_wheelVisuals = new Transform[WheelCount];
			m_rotationParts = new Transform[WheelCount];
			m_raycastDistance = new float[WheelCount];
			m_raycastMask = new int[WheelCount];
			m_baseWheelScale = new Vector3[WheelCount];
			m_baseRadius = new float[WheelCount];
			for (int i = 0; i < WheelCount; i++)
			{
				m_wheelRoots[i] = wheelRoots[i];
				m_wheelVisuals[i] = wheelVisuals[i];
				m_rotationParts[i] = wheelVisuals[i].GetChild(0);
				m_raycastMask[i] = desc.wheelInfos[i].raycastLayer.value;
				m_baseWheelScale[i] = m_rotationParts[i].localScale;
				m_baseRadius[i] = Mathf.Max(1e-3f, desc.wheelInfos[i].wheelRadius);
			}

			CacheAlignment();
			ApplyTrackWidth(desc);
			ApplyWheelRadius();

			if (!CreateSim()) return;

			m_wheelInput.wheels = new CarPhysicsNative.WheelState[WheelCount];

			SetupSkidAudio();

			if (m_input != null)
			{
				m_input.gearUp += OnGearUp;
				m_input.gearDown += OnGearDown;
			}
		}

		private void OnGearUp() => m_gearUpRequested = true;
		private void OnGearDown() => m_gearDownRequested = true;

		private ProfilerMarker m_marker = new ProfilerMarker("CarPhysics");

		private void FixedUpdate()
		{
			m_marker.Begin();
			if (m_rebuildRequested) Rebuild();
			if (m_sim == IntPtr.Zero) return;

			float dt = Time.fixedDeltaTime;

			var dIn = new CarPhysicsNative.DrivetrainInput
			{
				dt = dt,
				throttle = m_input != null ? m_input.acceleration : 0f,
				brake = m_input != null ? m_input.brakes : 0f,
				steer = m_input != null ? m_input.steering : 0f,
				clutch = m_input != null ? m_input.clutch : 0f,
				handbrake = m_input != null ? m_input.handbrake : 0f,
				gearUp = m_gearUpRequested ? 1 : 0,
				gearDown = m_gearDownRequested ? 1 : 0,
			};

			bool blockCar = m_input != null && m_input.blockCar;
			m_rb.constraints = blockCar
				? RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotationY
				: RigidbodyConstraints.None;
			m_gearUpRequested = false;
			m_gearDownRequested = false;

			CarPhysicsNative.carsim_update_drivetrain(m_sim, ref dIn, out m_drivetrainOutput);

			for (int i = 0; i < WheelCount; i++)
				m_wheelRoots[i].localRotation =
					Quaternion.Euler(0f, m_drivetrainOutput.steerAngles[i] + m_wheelToe[i], 0f);
			if (m_drivetrainOutput.applyNeutralTorque != 0)
				m_rb.AddTorque(m_drivetrainOutput.neutralBodyTorque.ToUnity());

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

			CarPhysicsNative.carsim_update_wheels(m_sim, ref m_wheelInput, out m_wheelOutput);

			for (int i = 0; i < WheelCount; i++)
			{
				if (m_wheelInput.wheels[i].hit != 0)
					m_rb.AddForceAtPosition(m_wheelOutput.applyForce[i].ToUnity(),
						m_wheelOutput.applyPoint[i].ToUnity());

				m_wheelVisuals[i].position = m_wheelOutput.visualPosition[i].ToUnity();
				m_rotationParts[i].localRotation = Quaternion.Euler(m_wheelOutput.spinEulerX[i], 0f, 0f);
				m_wheelVisuals[i].localRotation =
					Quaternion.Euler(0f, m_wheelOutput.steerEulerY[i], m_wheelCamber[i]);

				if (m_skidSources != null && m_skidSources[i] != null)
					m_skidSources[i].volume = Mathf.Clamp01(Mathf.Abs(m_wheelOutput.normalizedTireMagnitude[i]));
			}

			m_marker.End();
			CacheTelemetry();
		}

		private void CacheTelemetry()
		{
			engineRpm = m_drivetrainOutput.engineRpm;
			engineAngularVelocity = m_drivetrainOutput.engineAngularVelocity;
			clutchLock = m_drivetrainOutput.clutchLock;
			clutchTorque = m_drivetrainOutput.clutchTorque;
			currentGear = m_drivetrainOutput.currentGear;

			float dt = Time.fixedDeltaTime;
			for (int i = 0; i < WheelCount; i++)
			{
				m_suspensionForces[i] = m_wheelOutput.suspensionForce[i];
				m_angularVelocities[i] = m_wheelOutput.angularVelocity[i];
				m_slipAngles[i] = m_wheelOutput.slipAngle[i];
				m_slipForces[i] = new Vector2(m_wheelOutput.slipForceLong[i], m_wheelOutput.slipForceLat[i]);
				m_linearVelocities[i] = m_wheelOutput.linearVelocity[i].ToUnity();

				float len = m_wheelOutput.currentLength[i];
				Transform root = m_wheelRoots[i];
				m_telemetry[i] = new WheelTelemetry
				{
					slipRatio = m_wheelOutput.slipForceLong[i],
					slipAngleDeg = m_wheelOutput.slipForceLat[i],
					fx = m_wheelOutput.fx[i],
					fy = m_wheelOutput.fy != null ? m_wheelOutput.fy[i] : 0f,
					fz = m_wheelOutput.suspensionForce[i],
					normalizedMag = m_wheelOutput.normalizedTireMagnitude[i],
					suspensionLength = len,
					suspensionVel = (m_lastSuspensionLength[i] - len) / Mathf.Max(dt, 1e-5f),
					angularVelocity = m_wheelOutput.angularVelocity[i],
					grounded = m_wheelInput.wheels[i].hit != 0,
					contactPoint = m_wheelInput.wheels[i].hitPoint.ToUnity(),
					contactNormal = m_wheelInput.wheels[i].hitNormal.ToUnity(),
					wheelForward = root.forward,
					wheelRight = root.right,
					wheelUp = root.up,
				};
				m_lastSuspensionLength[i] = len;
			}
		}

		public void RequestRebuild() => m_rebuildRequested = true;

		private void Rebuild()
		{
			m_rebuildRequested = false;
			if (m_sim != IntPtr.Zero)
			{
				CarPhysicsNative.carsim_destroy(m_sim);
				m_sim = IntPtr.Zero;
			}
			engineMaxRpm = m_desc.engineInfo.engineMaxRpm;
			CacheAlignment();
			ApplyTrackWidth(m_desc);
			ApplyWheelRadius();
			CreateSim();
		}

		private bool CreateSim()
		{
			float wheelBase = Vector3.Distance(m_wheelRoots[0].position, m_wheelRoots[2].position);
			float rearTrack = Vector3.Distance(m_wheelRoots[2].position, m_wheelRoots[3].position);
			m_sim = CarPhysicsNative.Create(m_desc, wheelBase, rearTrack);
			if (m_sim == IntPtr.Zero)
			{
				Debug.LogError("CarPhysics: failed to create native simulation.");
				return false;
			}
			return true;
		}

		private void CacheAlignment()
		{
			for (int i = 0; i < WheelCount; i++)
			{
				var w = m_desc.wheelInfos[i];
				float side = (i % 2 == 0) ? -1f : 1f;
				m_wheelToe[i] = -side * w.toe;
				m_wheelCamber[i] = -w.camber;
				m_wheelCaster[i] = w.caster;
			}
		}

		private void ApplyWheelRadius()
		{
			for (int i = 0; i < WheelCount; i++)
			{
				var w = m_desc.wheelInfos[i];
				m_raycastDistance[i] = w.restLength + w.wheelRadius;
				if (m_baseWheelScale != null && m_baseRadius != null)
					m_rotationParts[i].localScale = m_baseWheelScale[i] * (w.wheelRadius / m_baseRadius[i]);
			}
		}

		private void ApplyTrackWidth(CarDesc desc)
		{
			for (int i = 0; i < WheelCount; i++)
			{
				float track = (i < 2) ? desc.trackFront : desc.trackRear;
				if (track <= 0f) continue;
				float side = (i % 2 == 0) ? -1f : 1f;
				Vector3 lp = m_wheelRoots[i].localPosition;
				lp.x = side * track * 0.5f;
				m_wheelRoots[i].localPosition = lp;
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
