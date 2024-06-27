using Car.Data;
using UnityEngine;

namespace Car.Models.PhysicsModels
{
    public class EngineModel
    {
        public const float RPM_TO_RAD = Mathf.PI * 2f / 60f;
        public const float RAD_TO_RPM = 1 / RPM_TO_RAD;

        private CarDesc.EngineInfo m_engineInfo;
        private Rigidbody m_carRigidbody;
        private float m_engineRpm;
        private float m_maxEffectiveTorque;
        private float m_engineTorque;
        private float m_engineFriction;
        private float m_engineAcceleration;
        private float m_engineAngularVelocity;

        public float engineAngularVelocity => m_engineAngularVelocity;
        public float engineRpm => m_engineRpm;
        public float engineMaxRpm => m_engineInfo.engineMaxRpm;
        public float carSpeed => m_carRigidbody.velocity.magnitude * 3.6f;

        public EngineModel(Rigidbody rb, CarDesc.EngineInfo engineInfo)
        {
            m_engineInfo = engineInfo;
            m_carRigidbody = rb;
            m_engineAngularVelocity = 100f;
        }
        
        public void UpdateEngine(float throttle, float clutchTorque, int currentGear)
        {
            m_maxEffectiveTorque = m_engineInfo.torqueCurve.Evaluate(m_engineRpm) * m_engineInfo.engineMul;
            m_engineFriction = (m_engineRpm * m_engineInfo.engineFrictionCoefficient) + m_engineInfo.startFriction;
            m_engineTorque = m_maxEffectiveTorque * throttle - m_engineFriction ;
            m_engineAcceleration = (m_engineTorque - clutchTorque) / m_engineInfo.engineInertia ;
            m_engineAngularVelocity += m_engineAcceleration * Time.fixedDeltaTime;
            m_engineRpm = m_engineAngularVelocity * RAD_TO_RPM;
            m_engineAngularVelocity = Mathf.Clamp(m_engineAngularVelocity, m_engineInfo.engineIdleRpm * RPM_TO_RAD, m_engineInfo.engineMaxRpm * RPM_TO_RAD);
        
            if (currentGear == 0)
            {
                m_carRigidbody.AddTorque(m_engineInfo.engineOrientation * m_engineTorque * 2f);
            }
        }
    }
}
