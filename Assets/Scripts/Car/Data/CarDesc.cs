using System;
using System.Collections.Generic;
using UnityEngine;

namespace Car.Data
{
    public class CarDesc : ScriptableObject
    {
        [Serializable]
        public class EngineInfo
        {
            private AnimationCurve m_torqueCurve;
            private Vector3 m_engineOrientation;
            private float m_engineIdleRpm;
            private float m_engineMaxRpm;
            private float m_engineMul;
            private float m_engineFrictionCoefficient;
            private float m_startFriction;
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

            private List<float> m_gearBoxRatios = new List<float>();
            private float m_shiftTime;
        }
        
        [Serializable]
        public class DifferentialInfo
        {
            private bool m_isDiffLocked;
            private float m_differentialRatio;

            public bool isDiffLocked => m_isDiffLocked;

            public float differentialRatio => m_differentialRatio;
        }

        [Serializable]
        public class ClutchInfo
        {
            private float m_clutchStiffness;
            private float m_clutchCapacity;
            private float m_clutchDamping;

            public float clutchStiffness => m_clutchStiffness;

            public float clutchCapacity => m_clutchCapacity;

            public float clutchDamping => m_clutchDamping;
        }
        
        [Serializable]
        public class WheelInfo
        {
            private float m_restLength;
            private float m_suspensionStiffness;
            private float m_damperStiffness;
            
            private float m_slipAnglePeak;

            private float m_camber;
            private float m_caster;
            
            private float m_longitudinalCoeff;
            private float m_lateralCoeff;

            private float m_wheelRadius;
            private LayerMask m_raycastLayer;
            
            private float m_wheelMass;
            private float m_longFrictionCoeff;
            
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
        }
        
        [Serializable]
        public class BrakesInfo
        {
            private AnimationCurve m_brakeTorqueCurve;
            private float m_maxTorque;

            public AnimationCurve brakeTorqueCurve => m_brakeTorqueCurve;

            public float maxTorque => m_maxTorque;
        }
    }
}
