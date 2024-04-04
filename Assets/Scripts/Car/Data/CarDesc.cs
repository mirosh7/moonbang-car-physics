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
            
            public AnimationCurve torqueCurve
            {
                get { return m_torqueCurve; }
            }

            public Vector3 engineOrientation
            {
                get { return m_engineOrientation; }
            }

            public float engineIdleRpm
            {
                get { return m_engineIdleRpm; }
            }

            public float engineMaxRpm
            {
                get { return m_engineMaxRpm; }
            }

            public float engineMul
            {
                get { return m_engineMul; }
            }

            public float engineFrictionCoefficient
            {
                get { return m_engineFrictionCoefficient; }
            }

            public float startFriction
            {
                get { return m_startFriction; }
            }

            public float engineInertia
            {
                get { return m_engineInertia; }
            }

            public float maxEngineTorque
            {
                get { return GetMaxValueFromAnimationCurve(m_torqueCurve); }
            }
            
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
            public List<float> gearBoxRatios
            {
                get { return m_gearBoxRatios; }
            }

            public float shiftTime
            {
                get { return m_shiftTime; }
            }

            private List<float> m_gearBoxRatios = new List<float>();
            private float m_shiftTime;
        }
        
        [Serializable]
        public class DifferentialInfo
        {
            private bool m_isDiffLocked;
            private float m_differentialRatio;

            public bool isDiffLocked
            {
                get { return m_isDiffLocked; }
            }

            public float differentialRatio
            {
                get { return m_differentialRatio; }
            }
        }

        [Serializable]
        public class ClutchInfo
        {
            private float m_clutchStiffness;
            private float m_clutchCapacity;
            private float m_clutchDamping;

            public float clutchStiffness
            {
                get { return m_clutchStiffness; }
            }

            public float clutchCapacity
            {
                get { return m_clutchCapacity; }
            }

            public float clutchDamping
            {
                get { return m_clutchDamping; }
            }
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
            
            public float longFrictionCoeff
            {
                get { return m_longFrictionCoeff; }
            }

            public float wheelInertia
            {
                get { return Mathf.Pow(wheelRadius, 2) * m_wheelMass; }
            }

            public LayerMask raycastLayer
            {
                get { return m_raycastLayer; }
            }

            public float wheelRadius
            {
                get { return m_wheelRadius; }
            }

            public float longitudinalCoeff
            {
                get { return m_longitudinalCoeff; }
            }

            public float lateralCoeff
            {
                get { return m_lateralCoeff; }
            }

            public float camber
            {
                get { return m_camber; }
            }

            public float caster
            {
                get { return m_caster; }
            }

            public float restLength
            {
                get { return m_restLength; }
            }

            public float suspensionStiffness
            {
                get { return m_suspensionStiffness; }
            }

            public float damperStiffness
            {
                get { return m_damperStiffness; }
            }

            public float slipAnglePeak
            {
                get { return m_slipAnglePeak; }
            }
        }
        
        [Serializable]
        public class BrakesInfo
        {
            private AnimationCurve m_brakeTorqueCurve;
            private float m_maxTorque;

            public AnimationCurve brakeTorqueCurve
            {
                get { return m_brakeTorqueCurve; }
            }

            public float maxTorque
            {
                get { return m_maxTorque; }
            }
        }
    }
}
