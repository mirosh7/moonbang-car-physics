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
        private float m_relaxationLength;
        
        public Vector2 slipForce => m_slipForce;
        public float slipAngle => m_slipAngle;

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
            const float suspensionForceThreshold = 0.05f;
            var targetAngularVelocity = linearVelocity.z / m_wheelRadius;
            var targetAngularAcceleration = (angularVelocity - targetAngularVelocity) / Time.fixedDeltaTime;
            var targetFrictionTorque = targetAngularAcceleration * m_wheelInertia;
            var maximumFrictionTorque = suspensionForce * m_wheelRadius * m_longFrictionCoeff;
            return suspensionForce < suspensionForceThreshold ? 0 : targetFrictionTorque / maximumFrictionTorque;
        }
        
        private float GetLateralForce(Vector3 linearVelocity)
        {
            const float slipAngleThreshold = 0.5f;
            float dynamicSlipAngle = 0f;
            m_slipAngle = Mathf.Abs(linearVelocity.z) < slipAngleThreshold  ? 0f : Mathf.Atan(-linearVelocity.x / Mathf.Abs(linearVelocity.z)) * Mathf.Rad2Deg;
            //return m_slipAngle / m_slipAnglePeak;
            //Transient force calc
            var coeff = (Mathf.Abs(linearVelocity.x) / m_relaxationLength) * Time.fixedDeltaTime;
            coeff = Mathf.Clamp(coeff, 0f, 1f);
            dynamicSlipAngle += (m_slipAngle - dynamicSlipAngle) * coeff;
            dynamicSlipAngle = Mathf.Clamp(dynamicSlipAngle, -90f, 90f);
            return Mathf.Clamp(dynamicSlipAngle / m_slipAnglePeak, -1, 1);
        }

        public void UpdateSlipForces(Vector3 linearVelocity, float suspensionForce, float angularVelocity)
        {
            m_slipForce = new Vector2(GetLongitudinalForce(linearVelocity, suspensionForce, angularVelocity),
                GetLateralForce(linearVelocity));
        }
    }
}
