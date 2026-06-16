using System;
using System.Collections.Generic;
using Car.Models;
using UnityEngine;
using UnityEngine.Serialization;

namespace Car.Data
{
    [CreateAssetMenu(fileName = "Car Description",menuName = "Scriptable Objects/Car Description")]
    public class CarDesc : ScriptableObject
    {
        [SerializeField]
        private EngineInfo m_engineInfo;
        [SerializeField]
        private GearboxInfo m_gearboxInfo;
        [SerializeField]
        private ClutchInfo m_clutchInfo;
        [SerializeField]
        private DifferentialInfo m_differentialInfo;
        [SerializeField]
        private BrakesInfo m_brakesInfo;
        [SerializeField]
        private List<WheelInfo> m_wheelInfos;
        [SerializeField]
        private SteeringInfo m_steeringInfo;
        [SerializeField]
        private AntirollBarInfo m_antirollBarInfo;

        [Header("Колея (расстояние между стойками)")]
        [Tooltip("Передняя колея, м. 0 — не двигать wheelRoot, оставить как в префабе.")]
        [SerializeField]
        private float m_trackFront;
        [Tooltip("Задняя колея, м. 0 — не двигать wheelRoot, оставить как в префабе.")]
        [SerializeField]
        private float m_trackRear;

        public float trackFront { get => m_trackFront; set => m_trackFront = value; }
        public float trackRear { get => m_trackRear; set => m_trackRear = value; }

        public AntirollBarInfo antirollBarInfo => m_antirollBarInfo;
        public EngineInfo engineInfo => m_engineInfo;
        public GearboxInfo gearboxInfo => m_gearboxInfo;
        public ClutchInfo clutchInfo => m_clutchInfo;
        public DifferentialInfo differentialInfo => m_differentialInfo;
        public BrakesInfo brakesInfo => m_brakesInfo;
        public List<WheelInfo> wheelInfos => m_wheelInfos;
        public SteeringInfo steeringInfo => m_steeringInfo;
     
            
        [Serializable]
        public class EngineInfo
        {
            [SerializeField]
            private AnimationCurve m_torqueCurve;
            [SerializeField]
            private Vector3 m_engineOrientation;
            [SerializeField]
            private float m_engineIdleRpm;
            [SerializeField]
            private float m_engineMaxRpm;
            [SerializeField]
            private float m_engineMul;
            [SerializeField]
            private float m_engineFrictionCoefficient;
            [SerializeField]
            private float m_startFriction;
            [SerializeField]
            private float m_engineInertia;
            
            public AnimationCurve torqueCurve => m_torqueCurve;

            public Vector3 engineOrientation => m_engineOrientation;

            public float engineIdleRpm => m_engineIdleRpm;

            public float engineMaxRpm => m_engineMaxRpm;

            public float engineMul => m_engineMul;

            public float engineFrictionCoefficient => m_engineFrictionCoefficient;

            public float startFriction => m_startFriction;

            public float engineInertia => m_engineInertia;

            public float maxEngineTorque => GetMaxValueFromAnimationCurve(m_torqueCurve);

            private float GetMaxValueFromAnimationCurve(AnimationCurve curve)
            {
                float maxValue = float.MinValue;

                for (int i = 0; i < curve.length; i++)
                {
                    float keyValue = curve.keys[i].value;

                    if (keyValue > maxValue)
                    {
                        maxValue = keyValue;
                    }
                }

                return maxValue;
            }
        }
        
        [Serializable]
        public class GearboxInfo
        {
            public List<float> gearBoxRatios => m_gearBoxRatios;

            public float shiftTime => m_shiftTime;

            [SerializeField]
            private List<float> m_gearBoxRatios = new List<float>();
            [SerializeField]
            private float m_shiftTime;
        }
        
        // Values must match the CP_DRIVE_* / CP_DIFF_* enums in car_physics.h.
        public enum DriveMode { FWD = 0, RWD = 1, AWD = 2 }
        public enum DiffType { Open = 0, Locked = 1, LSD = 2 }

        [Serializable]
        public class DifferentialInfo
        {
            [SerializeField]
            private DriveMode m_driveMode = DriveMode.RWD;
            [SerializeField]
            private DiffType m_diffType = DiffType.Locked;
            [SerializeField]
            private float m_differentialRatio = 3.44f;
            [Tooltip("AWD: доля момента на переднюю ось (0..1).")]
            [Range(0f, 1f)]
            [SerializeField]
            private float m_torqueSplitFront = 0.4f;
            [Tooltip("LSD: преднатяг блокировки, Н·м на рад/с разницы скоростей колёс.")]
            [SerializeField]
            private float m_lockingCoeff = 80f;

            public DriveMode driveMode { get => m_driveMode; set => m_driveMode = value; }
            public DiffType diffType { get => m_diffType; set => m_diffType = value; }
            public float differentialRatio { get => m_differentialRatio; set => m_differentialRatio = value; }
            public float torqueSplitFront { get => m_torqueSplitFront; set => m_torqueSplitFront = value; }
            public float lockingCoeff { get => m_lockingCoeff; set => m_lockingCoeff = value; }
        }

        [Serializable]
        public class ClutchInfo
        {
            [SerializeField]
            private float m_clutchStiffness;
            [SerializeField]
            private float m_clutchCapacity;
            [SerializeField]
            private float m_clutchDamping;

            public float clutchStiffness => m_clutchStiffness;

            public float clutchCapacity => m_clutchCapacity;

            public float clutchDamping => m_clutchDamping;
        }
        
        [Serializable]
        public class WheelInfo
        {
            [SerializeField]
            private float m_restLength;
            [SerializeField]
            private float m_suspensionStiffness;
            [SerializeField]
            private float m_damperStiffness;
            [SerializeField]
            private float m_slipAnglePeak;
            [Tooltip("Статический развал, град. >0 — верх колеса наружу.")]
            [SerializeField]
            private float m_camber;
            [Tooltip("Кастер (продольный наклон оси поворота), град. Даёт самовыравнивание и набор развала в повороте.")]
            [SerializeField]
            private float m_caster;
            [Tooltip("Схождение, град на колесо. >0 — toe-in (носки внутрь).")]
            [SerializeField]
            private float m_toe;
            [Tooltip("Поперечный наклон оси поворота (kingpin), град.")]
            [SerializeField]
            private float m_kingpinInclination;
            [Tooltip("Коэффициент camber thrust: Fy_развал = coeff * sin(развал) * Fz.")]
            [SerializeField]
            private float m_camberCoeff = 0.6f;
            [SerializeField]
            private float m_longitudinalCoeff;
            [SerializeField]
            private float m_lateralCoeff;
            [SerializeField]
            private float m_wheelRadius;
            [SerializeField]
            private LayerMask m_raycastLayer;
            [SerializeField]
            private float m_wheelMass;
            [SerializeField]
            private float m_longFrictionCoeff;

            [SerializeField]
            private float m_relaxationLength;

            [Header("Pacejka Magic Formula")]
            [Tooltip("Slip ratio at peak longitudinal force (e.g. 0.12).")]
            [SerializeField]
            private float m_longSlipPeak = 0.12f;
            [Tooltip("Longitudinal shape factor C_x (e.g. 1.65).")]
            [SerializeField]
            private float m_pacejkaShapeLong = 1.65f;
            [Tooltip("Longitudinal curvature factor E_x (e.g. 0.96).")]
            [SerializeField]
            private float m_pacejkaCurveLong = 0.96f;
            [Tooltip("Lateral shape factor C_y (e.g. 1.35).")]
            [SerializeField]
            private float m_pacejkaShapeLat = 1.35f;
            [Tooltip("Lateral curvature factor E_y (e.g. 0.96).")]
            [SerializeField]
            private float m_pacejkaCurveLat = 0.96f;

            public float longSlipPeak { get => m_longSlipPeak; set => m_longSlipPeak = value; }
            public float pacejkaShapeLong { get => m_pacejkaShapeLong; set => m_pacejkaShapeLong = value; }
            public float pacejkaCurveLong { get => m_pacejkaCurveLong; set => m_pacejkaCurveLong = value; }
            public float pacejkaShapeLat { get => m_pacejkaShapeLat; set => m_pacejkaShapeLat = value; }
            public float pacejkaCurveLat { get => m_pacejkaCurveLat; set => m_pacejkaCurveLat = value; }

            public float longFrictionCoeff => m_longFrictionCoeff;

            public float wheelInertia => Mathf.Pow(wheelRadius, 2) * m_wheelMass;

            public LayerMask raycastLayer => m_raycastLayer;

            public float wheelRadius { get => m_wheelRadius; set => m_wheelRadius = value; }

            public float longitudinalCoeff { get => m_longitudinalCoeff; set => m_longitudinalCoeff = value; }

            public float lateralCoeff { get => m_lateralCoeff; set => m_lateralCoeff = value; }

            public float camber { get => m_camber; set => m_camber = value; }

            public float caster { get => m_caster; set => m_caster = value; }

            public float toe { get => m_toe; set => m_toe = value; }

            public float kingpinInclination { get => m_kingpinInclination; set => m_kingpinInclination = value; }

            public float camberCoeff { get => m_camberCoeff; set => m_camberCoeff = value; }

            public float restLength { get => m_restLength; set => m_restLength = value; }

            public float suspensionStiffness { get => m_suspensionStiffness; set => m_suspensionStiffness = value; }

            public float damperStiffness { get => m_damperStiffness; set => m_damperStiffness = value; }

            public float slipAnglePeak { get => m_slipAnglePeak; set => m_slipAnglePeak = value; }

            public float relaxationLength { get => m_relaxationLength; set => m_relaxationLength = value; }
        }
        
        [Serializable]
        public class SteeringInfo
        {
            [SerializeField]
            private float m_turnRadius;
            [SerializeField]
            private float m_steerForce;
            [SerializeField]
            private float m_maxCorrectionAngle;
            [SerializeField]
            private float m_correctionSpeed;

            public float turnRadius => m_turnRadius;
            public float steerForce => m_steerForce;
            public float maxCorrectionAngle => m_maxCorrectionAngle;
            public float correctionSpeed => m_correctionSpeed;

        }
        
        [Serializable]
        public class BrakesInfo
        {
            [SerializeField]
            private AnimationCurve m_brakeTorqueCurve;
            [SerializeField]
            private float m_maxTorque;

            [SerializeField]
            private List<float> m_brakeBias;
            [Tooltip("Момент ручника на задние колёса, Н·м.")]
            [SerializeField]
            private float m_handbrakeTorque = 4000f;

            public AnimationCurve brakeTorqueCurve => m_brakeTorqueCurve;

            public float maxTorque { get => m_maxTorque; set => m_maxTorque = value; }

            public float handbrakeTorque { get => m_handbrakeTorque; set => m_handbrakeTorque = value; }

            public List<float> brakeBias => m_brakeBias;
        }
        
        [Serializable]
        public class AntirollBarInfo
        {
            [SerializeField]
            private bool m_isEnabled;
            [SerializeField]
            private float m_stiffnessFront;
            [SerializeField]
            private float m_stiffnessRear;

            public bool isEnabled { get => m_isEnabled; set => m_isEnabled = value; }
            public float stiffnessFront { get => m_stiffnessFront; set => m_stiffnessFront = value; }
            public float stiffnessRear { get => m_stiffnessRear; set => m_stiffnessRear = value; }
        }
    }
}
