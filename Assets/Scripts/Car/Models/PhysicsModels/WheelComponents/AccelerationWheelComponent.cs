using Car.Data;
using UnityEngine;

namespace Car.Models.PhysicsModels.WheelComponents
{
    public class AccelerationWheelComponent
    {
        private float m_angularVelocity;
        private float m_wheelRadius;
        private float m_wheelInertia;
        private float m_brakeVelocity;

        public AccelerationWheelComponent(CarDesc.WheelInfo wheelInfo)
        {
            m_wheelRadius = wheelInfo.wheelRadius;
            m_wheelInertia = wheelInfo.wheelInertia;
        }
        
        public float angularVelocity => m_angularVelocity;

        public void UpdateWheelAcceleration(float fX, float driveTorque, float brakeTorque)
        {
            var frictionTorque = fX * m_wheelRadius;
            var angularAcceleration = (driveTorque - frictionTorque) / m_wheelInertia;
            var brakeForce = Mathf.Abs(brakeTorque) * Mathf.Sign(m_angularVelocity) / m_wheelInertia;
            var brakeSmoothTime = 0.2f; // Время сглаживания (можно настроить)
            var smoothedBrakeForce = Mathf.SmoothDamp(0f, brakeForce, ref m_brakeVelocity, brakeSmoothTime);
            m_angularVelocity += angularAcceleration * Time.fixedDeltaTime;
            m_angularVelocity -= smoothedBrakeForce * Time.fixedDeltaTime;
            m_angularVelocity = Mathf.Clamp(m_angularVelocity, -360, 360);
            
            if (Mathf.Abs(brakeForce) >= Mathf.Abs(m_angularVelocity))
            {
                m_angularVelocity = 0f;
            }
        }
    }
}
