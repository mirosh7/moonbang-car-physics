using Car.Data;
using UnityEngine;

namespace Car.Models.WheelComponents
{
    public class AccelerationWheelComponent
    {
        private float m_angularVelocity;
        private float m_wheelRadius;
        private float m_wheelInertia;

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
            m_angularVelocity += angularAcceleration * Time.fixedDeltaTime;
            m_angularVelocity = Mathf.Clamp(m_angularVelocity, -360, 360);
            m_angularVelocity -= Mathf.Min(Mathf.Abs(m_angularVelocity), brakeTorque * Mathf.Sign(m_angularVelocity) / m_wheelInertia * Time.fixedDeltaTime);
        }
    }
}
