using Car.Core;
using UnityEngine;

namespace Car.CarControllers.WheelControllers
{
    public class WheelSteeringController : IController
    {
        private WheelController m_wheel;
        private float m_steeringAngle;
        
        public WheelSteeringController(WheelController wheel)
        {
            m_wheel = wheel;
        }
        
        public void RotateWheel(float angle, float steerTime)
        {
            m_steeringAngle = Mathf.Lerp(m_steeringAngle, angle, Time.fixedDeltaTime * steerTime);
            m_wheel.transform.localRotation = Quaternion.Euler(0f, m_steeringAngle, 0f);
        }
    }
}
