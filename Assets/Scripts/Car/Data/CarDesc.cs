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
        
        [Serializable]
        public class DifferentialInfo
        {
            [SerializeField]
            private bool m_isDiffLocked;
            [SerializeField]
            private float m_differentialRatio;

            public bool isDiffLocked => m_isDiffLocked;

            public float differentialRatio => m_differentialRatio;
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
            [SerializeField]
            private float m_camber;
            [SerializeField]
            private float m_caster;
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

            public float longSlipPeak => m_longSlipPeak;
            public float pacejkaShapeLong => m_pacejkaShapeLong;
            public float pacejkaCurveLong => m_pacejkaCurveLong;
            public float pacejkaShapeLat => m_pacejkaShapeLat;
            public float pacejkaCurveLat => m_pacejkaCurveLat;

            public float longFrictionCoeff => m_longFrictionCoeff;

            public float wheelInertia => Mathf.Pow(wheelRadius, 2) * m_wheelMass;

            public LayerMask raycastLayer => m_raycastLayer;

            public float wheelRadius => m_wheelRadius;

            public float longitudinalCoeff => m_longitudinalCoeff;

            public float lateralCoeff => m_lateralCoeff;

            public float camber => m_camber;

            public float caster => m_caster;

            public float restLength => m_restLength;

            public float suspensionStiffness => m_suspensionStiffness;

            public float damperStiffness => m_damperStiffness;

            public float slipAnglePeak => m_slipAnglePeak;

            public float relaxationLength => m_relaxationLength;
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

            public AnimationCurve brakeTorqueCurve => m_brakeTorqueCurve;

            public float maxTorque => m_maxTorque;

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

            public bool isEnabled => m_isEnabled;
            public float stiffnessFront => m_stiffnessFront;
            public float stiffnessRear => m_stiffnessRear;
        }
    }
}
