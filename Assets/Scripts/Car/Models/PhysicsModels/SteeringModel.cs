using System.Collections.Generic;
using Car.Data;
using UnityEngine;

namespace Car.Models.PhysicsModels
{
    public class SteeringModel
    {
        private float m_wheelBase;
        private float m_rearTrack;
        private float m_turnRadius;
        private float m_ackermannAngleL;
        private float m_ackermannAngleR;
        private List<float> m_steerAngles = new List<float>(4);
        private float m_steerForce;

        public SteeringModel(CarDesc.SteeringInfo steeringInfo, List<Transform> wheelTransform)
        {
            m_turnRadius = steeringInfo.turnRadius;
            m_steerForce = steeringInfo.steerForce;

            foreach (var wheel in wheelTransform)
            {
                m_steerAngles.Add(0f);
            }
            
            InitializeAckermannParams(wheelTransform);
        }

        public List<float> steerAngles => m_steerAngles;

        public void UpdateAckermann(float inputSteering, List<float> slipAngles)
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
            
            m_steerAngles[0] = m_ackermannAngleR;
            m_steerAngles[1] = m_ackermannAngleL;
        }
        
        private void InitializeAckermannParams(List<Transform> wheelTransform)
        {
            m_wheelBase = Vector3.Distance(wheelTransform[0].position, wheelTransform[2].position);
            m_rearTrack = Vector3.Distance(wheelTransform[2].position, wheelTransform[3].position);  
        }
    }
}
