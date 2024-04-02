using Car.Core;
using UnityEngine;

namespace Car.CarControllers.WheelControllers
{
    public class SlipForceController : IController
    {
        private Vector3 m_linearVelocity;
        private float m_longFrictionCoefficient;
        private float m_wheelInertia;
        private float m_angularVelocity;
        private float m_longitudinalForce;
        private float m_lateralForceForce;
        private float m_slipAnglePeak;
        private float m_wheelRadius;

        public Vector2 slipForce => new Vector2(m_longitudinalForce, m_lateralForceForce);
        
        private void GetSx(float fZ)
        {
            var targetAngularVelocity = m_linearVelocity.z / m_wheelRadius;
            var targetAngularAcceleration = (m_angularVelocity - targetAngularVelocity) / Time.fixedDeltaTime;
            var targetFrictionTorque = targetAngularAcceleration * m_wheelInertia;
            var maximumFrictionTorque = fZ * m_wheelRadius * m_longFrictionCoefficient;
            m_longitudinalForce = fZ == 0 ? 0 : targetFrictionTorque / maximumFrictionTorque;
       

        }

        private void GetSy()
        {
            var slipAngle = Mathf.Abs(m_linearVelocity.z) <=0.5  ? 0 : Mathf.Atan(-m_linearVelocity.x / Mathf.Abs(m_linearVelocity.z)) * Mathf.Rad2Deg;
            /* coeff = (Mathf.Abs(linearVelocity.x) / relaxationLenth) * deltaTime;
             coeff = Mathf.Clamp(coeff, 0f, 1f);
             SADyn += (slipAngle - SADyn) * coeff;
             SADyn = Mathf.Clamp(SADyn, -90f, 90f);
             sY = Mathf.Clamp(SADyn / slipAnglePeak, -1, 1);*/
            m_lateralForceForce = slipAngle / m_slipAnglePeak;
        }
        
    }
}
