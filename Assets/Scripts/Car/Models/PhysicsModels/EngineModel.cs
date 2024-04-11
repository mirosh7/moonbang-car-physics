using Car.Data;
using UnityEngine;

namespace Car.Models.PhysicsModels
{
    public class EngineModel
    {
        public const float RPM_TO_RAD = Mathf.PI * 2f / 60f;
        public const float RAD_TO_RPM = 1 / RPM_TO_RAD;
    
        private Rigidbody m_carRigidbody;
        private AnimationCurve m_torqueCurve;

        private Vector3 m_engineOrientation;
    
        private float m_engineIdleRpm;
        private float m_engineRpm;
        private float m_engineMaxRpm;
    
        private float m_maxEffectiveTorque;
        private float m_engineTorque;
        private float m_engineMul;
        private float m_engineFriction;
        private float m_engineFrictionCoefficient;
        private float m_engineAcceleration;
        private float m_engineAngularVelocity;
        private float m_startFriction;
        private float m_engineInertia;

        public float engineAngularVelocity => m_engineAngularVelocity;
        public float engineRpm => m_engineRpm;
        public float carSpeed => m_carRigidbody.velocity.magnitude * 3.6f;

        public EngineModel(Rigidbody rb, CarDesc.EngineInfo engineInfo)
        {
            m_carRigidbody = rb;
            m_torqueCurve = engineInfo.torqueCurve;
            m_engineOrientation = engineInfo.engineOrientation;
            m_engineIdleRpm = engineInfo.engineIdleRpm * RPM_TO_RAD;
            m_engineMaxRpm = engineInfo.engineMaxRpm * RPM_TO_RAD;
            m_engineMul = engineInfo.engineMul;
            m_engineFrictionCoefficient = engineInfo.engineFrictionCoefficient;
            m_startFriction = engineInfo.startFriction;
            m_engineInertia = engineInfo.engineInertia;
            m_engineAngularVelocity = 100f;
        }
        
        public void UpdateEngine(float throttle, float clutchTorque, int currentGear)
        {
            m_maxEffectiveTorque = m_torqueCurve.Evaluate(m_engineRpm) * m_engineMul;
            m_engineFriction = (m_engineRpm * m_engineFrictionCoefficient) + m_startFriction;
            m_engineTorque = m_maxEffectiveTorque * throttle - m_engineFriction ;
            m_engineAcceleration = (m_engineTorque - clutchTorque) / m_engineInertia ;
            m_engineAngularVelocity += m_engineAcceleration * Time.fixedDeltaTime;
            m_engineRpm = m_engineAngularVelocity * RAD_TO_RPM;
            m_engineAngularVelocity = Mathf.Clamp(m_engineAngularVelocity, m_engineIdleRpm, m_engineMaxRpm);
        
            if (currentGear == 0)
            {
                m_carRigidbody.AddTorque(m_engineOrientation * m_engineTorque * 2f);
            }
        }
    }
}
