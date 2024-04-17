using System;
using Car.Data;
using UnityEngine;
using UnityEngine.Rendering;

namespace Car.Models.PhysicsModels.WheelComponents
{
    public class SlipForceComponent
    {
        private float m_slipAnglePeak;
        private float m_wheelInertia;
        private float m_wheelRadius;
        private float m_longFrictionCoeff;
        private Vector2 m_slipForce;
        private float m_slipAngle;
        private float m_dynamicSlipAngle;
        private float m_relaxationLength;
        private float m_lateralAcceleration;
        
        public Vector2 slipForce => m_slipForce;
        public float slipAngle => m_slipAngle;
        public float lateralAcceleration => m_lateralAcceleration;

        public SlipForceComponent(CarDesc.WheelInfo wheelInfo)
        {
            m_slipAnglePeak = wheelInfo.slipAnglePeak;
            m_wheelInertia = wheelInfo.wheelInertia;
            m_wheelRadius = wheelInfo.wheelRadius;
            m_longFrictionCoeff = wheelInfo.longFrictionCoeff;
            m_relaxationLength = wheelInfo.relaxationLength;
        }

        private float GetLongitudinalForce(Vector3 linearVelocity, float suspensionForce, float angularVelocity)
        {
            const float suspensionForceThreshold = Single.Epsilon;
            var targetAngularVelocity = linearVelocity.z / m_wheelRadius;
            var targetAngularAcceleration = (angularVelocity - targetAngularVelocity) / Time.fixedDeltaTime;
            var targetFrictionTorque = targetAngularAcceleration * m_wheelInertia;
            var maximumFrictionTorque = suspensionForce * m_wheelRadius * m_longFrictionCoeff;
            return suspensionForce < suspensionForceThreshold ? 0 : targetFrictionTorque / maximumFrictionTorque;
        }
        
        private float GetLateralForce(Vector3 linearVelocity)
        {
            const float slipAngleThreshold = Single.Epsilon;
            m_slipAngle = Mathf.Abs(linearVelocity.z) < slipAngleThreshold  ? 0f : Mathf.Atan(-linearVelocity.x / Mathf.Abs(linearVelocity.z)) * Mathf.Rad2Deg;
            var coeff = (Mathf.Abs(linearVelocity.x) / m_relaxationLength) * Time.fixedDeltaTime;
            coeff = Mathf.Clamp(coeff, 0f, 1f);
            m_dynamicSlipAngle += (m_slipAngle - m_dynamicSlipAngle) * coeff;
            m_dynamicSlipAngle = Mathf.Clamp(m_dynamicSlipAngle, -90f, 90f);
            return m_dynamicSlipAngle / m_slipAnglePeak;
        }

        private void UpdateLateralAcceleration(Vector3 linearVelocity)
        {
            m_lateralAcceleration = (Mathf.Pow(linearVelocity.magnitude, 2) / m_wheelRadius) * Mathf.Tan(m_slipAngle * Mathf.Deg2Rad);
        }

        public void UpdateSlipForces(Vector3 linearVelocity, float suspensionForce, float angularVelocity)
        {
            m_slipForce = new Vector2(GetLongitudinalForce(linearVelocity, suspensionForce, angularVelocity),
                GetLateralForce(linearVelocity));
            
            UpdateLateralAcceleration(linearVelocity);
        }
    }
}
