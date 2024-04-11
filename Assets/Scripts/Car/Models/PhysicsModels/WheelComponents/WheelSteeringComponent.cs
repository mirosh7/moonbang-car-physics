using UnityEngine;

namespace Car.Models.PhysicsModels.WheelComponents
{
    public class WheelSteeringComponent
    {
        private Transform m_wheelRoot;

        public WheelSteeringComponent(Transform wheelRoot)
        {
            m_wheelRoot = wheelRoot;
        }

        public void UpdateWheelSteer(float steerAngle)
        {
            m_wheelRoot.localRotation = Quaternion.Euler(0, steerAngle, 0f);
        }
    }
}
