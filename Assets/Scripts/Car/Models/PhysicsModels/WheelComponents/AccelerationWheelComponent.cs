using System;
using Car.Data;
using UnityEngine;

namespace Car.Models.PhysicsModels.WheelComponents
{
    public class AccelerationWheelComponent
    {
        private CarDesc.WheelInfo m_wheelInfo;
        private float m_angularVelocity;
        private float m_brakeVelocity;

        public AccelerationWheelComponent(CarDesc.WheelInfo wheelInfo)
        {
            m_wheelInfo = wheelInfo;
        }
        
        public float angularVelocity => m_angularVelocity;

        public void UpdateWheelAcceleration(float fX, float driveTorque, float brakeTorque)
        {
            var frictionTorque = fX * m_wheelInfo.wheelRadius;
            var angularAcceleration = (driveTorque - frictionTorque) / m_wheelInfo.wheelInertia;
            m_angularVelocity += angularAcceleration * Time.fixedDeltaTime;
            m_angularVelocity -= Mathf.Min(Mathf.Abs(m_angularVelocity), brakeTorque * Mathf.Sign(m_angularVelocity) / m_wheelInfo.wheelInertia * Time.fixedDeltaTime);
            m_angularVelocity = Mathf.Clamp(m_angularVelocity, -360f, 360f);
        }
    }
}
