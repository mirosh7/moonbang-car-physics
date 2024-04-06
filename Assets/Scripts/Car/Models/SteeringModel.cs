using Car.Data;
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
        private float[] m_steerAngle;
        private float m_steerForce;

        public SteeringModel(CarDesc.SteeringInfo steeringInfo, Transform[] wheelTransform)
        {
            m_turnRadius = steeringInfo.turnRadius;
            m_steerForce = steeringInfo.steerForce;
            InitializeAckermannParams(wheelTransform);
        }

        public float[] steerAngle => m_steerAngle;

        public void UpdateAckermann(float inputSteering)
        {
            if (inputSteering > 0)
            {
                m_ackermannAngleL = Mathf.Rad2Deg * Mathf.Atan(m_wheelBase / (m_turnRadius + m_rearTrack / 2)) * inputSteering * m_steerForce;
                m_ackermannAngleR = Mathf.Rad2Deg * Mathf.Atan(m_wheelBase / (m_turnRadius - m_rearTrack / 2)) * inputSteering * m_steerForce;
            }
            else if (inputSteering < 0)
            {
                m_ackermannAngleL = Mathf.Rad2Deg * Mathf.Atan(m_wheelBase / (m_turnRadius - m_rearTrack / 2)) * inputSteering * m_steerForce;
                m_ackermannAngleR = Mathf.Rad2Deg * Mathf.Atan(m_wheelBase / (m_turnRadius + m_rearTrack / 2)) * inputSteering * m_steerForce;
            }
            else
            {
                m_ackermannAngleL = 0f;
                m_ackermannAngleR = 0f;
            }
            
            m_steerAngle[0] = m_ackermannAngleL;
            m_steerAngle[1] = m_ackermannAngleR;
        }
        
        private void InitializeAckermannParams(Transform[] wheelTransform)
        {
            m_wheelBase = Vector3.Distance(wheelTransform[0].position, wheelTransform[2].position);
            m_rearTrack = Vector3.Distance(wheelTransform[2].position, wheelTransform[3].position);  
        }
    }
}
