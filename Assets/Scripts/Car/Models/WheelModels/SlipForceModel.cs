using Car.Data;
using UnityEngine;

namespace Car.Models.WheelModels
{
    public class SlipForceModel
    {
        private float m_slipAnglePeak;
        private float m_wheelInertia;
        private float m_wheelRadius;
        private float m_longFrictionCoeff;

        public SlipForceModel(CarDesc.WheelInfo wheelInfo)
        {
            m_slipAnglePeak = wheelInfo.slipAnglePeak;
            m_wheelInertia = wheelInfo.wheelInertia;
            m_wheelRadius = wheelInfo.wheelRadius;
            m_longFrictionCoeff = wheelInfo.longFrictionCoeff;
        }

        public float GetLongitudinalForce(Vector3 linearVelocity, float suspensionForce, float angularVelocity)
        {
            var targetAngularVelocity = linearVelocity.z / m_wheelRadius;
            var targetAngularAcceleration = (angularVelocity - targetAngularVelocity) / Time.fixedDeltaTime;
            var targetFrictionTorque = targetAngularAcceleration * m_wheelInertia;
            var maximumFrictionTorque = suspensionForce * m_wheelRadius * m_longFrictionCoeff;
            return suspensionForce == 0 ? 0 : targetFrictionTorque / maximumFrictionTorque;
        }

        public float GetLateralForce(Vector3 linearVelocity)
        {
            var slipAngle = Mathf.Abs(linearVelocity.z) <= 0.5  ? 0 : Mathf.Atan(-linearVelocity.x / Mathf.Abs(linearVelocity.z)) * Mathf.Rad2Deg;
            return slipAngle / m_slipAnglePeak;
            //Transient force calc
            /* coeff = (Mathf.Abs(linearVelocity.x) / relaxationLenth) * deltaTime;
             coeff = Mathf.Clamp(coeff, 0f, 1f);
             SADyn += (slipAngle - SADyn) * coeff;
             SADyn = Mathf.Clamp(SADyn, -90f, 90f);
             return Mathf.Clamp(SADyn / slipAnglePeak, -1, 1);*/ 
        }
    }
}
