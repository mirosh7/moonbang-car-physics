using UnityEngine;

namespace Car.Models
{
    public class SteeringModel
    {
        private float m_wheelBase;
        private float m_rearTrack;
        private float m_turnRadius;
        private float m_ackermannAngleL;
        private float m_ackermannAngleR;
        private readonly float[] steerAngle;
        private readonly float steerForce;

        public float[] steerAngle1 => steerAngle;

        private void UpdateAckermann(float inputSteering)
        {
            if (inputSteering > 0)
            {
                m_ackermannAngleL = Mathf.Rad2Deg * Mathf.Atan(m_wheelBase / (m_turnRadius + m_rearTrack / 2)) * inputSteering * steerForce;
                m_ackermannAngleR = Mathf.Rad2Deg * Mathf.Atan(m_wheelBase / (m_turnRadius - m_rearTrack / 2)) * inputSteering * steerForce;
            }
            else if (inputSteering < 0)
            {
                m_ackermannAngleL = Mathf.Rad2Deg * Mathf.Atan(m_wheelBase / (m_turnRadius - m_rearTrack / 2)) * inputSteering * steerForce;
                m_ackermannAngleR = Mathf.Rad2Deg * Mathf.Atan(m_wheelBase / (m_turnRadius + m_rearTrack / 2)) * inputSteering * steerForce;
            }
            else
            {
                m_ackermannAngleL = 0f;
                m_ackermannAngleR = 0f;
            }
            
            steerAngle[0] = m_ackermannAngleL;
            steerAngle[1] = m_ackermannAngleR;
        }
    }
}
