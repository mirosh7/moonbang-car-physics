using System.Collections.Generic;
using System.Linq;
using Car.Data;
using UnityEngine;

namespace Car.Models.PhysicsModels
{
    public class SteeringModel
    {
        private CarDesc.SteeringInfo m_steeringInfo;
        private float m_wheelBase;
        private float m_rearTrack;
        private float m_ackermannAngleL;
        private float m_ackermannAngleR;
        private List<float> m_steerAngles = new List<float>(4);
        private float m_carSteerInput;

        public SteeringModel(CarDesc.SteeringInfo steeringInfo, List<Transform> wheelTransform)
        {
            m_steeringInfo = steeringInfo;
            
            foreach (var wheel in wheelTransform)
            {
                m_steerAngles.Add(0f);
            }
            
            InitializeAckermannParams(wheelTransform);
        }

        public List<float> steerAngles => m_steerAngles;

        private void UpdateAckermann()
        {
            m_ackermannAngleL = Mathf.Rad2Deg * Mathf.Atan(m_wheelBase / (m_carSteerInput > 0 ? m_steeringInfo.turnRadius + m_rearTrack / 2 : m_steeringInfo.turnRadius - m_rearTrack / 2)) * m_carSteerInput * m_steeringInfo.steerForce;
            m_ackermannAngleR = Mathf.Rad2Deg * Mathf.Atan(m_wheelBase / (m_carSteerInput > 0 ? m_steeringInfo.turnRadius - m_rearTrack / 2 : m_steeringInfo.turnRadius + m_rearTrack / 2)) * m_carSteerInput * m_steeringInfo.steerForce;
        }

        private void RawSteeringInputLerp(float inputSteering)
        {
            m_carSteerInput = inputSteering;
        }

        private void UpdateCorrection(List<float> slipAngles, List<float> lateralAccelerations)
        {
            float correctionFactor = m_steeringInfo.correctionSpeed * Time.fixedDeltaTime;
            m_ackermannAngleL -= lateralAccelerations[1] * correctionFactor;
            m_ackermannAngleR -= lateralAccelerations[0] * correctionFactor;
                
            m_ackermannAngleL = Mathf.Clamp(m_ackermannAngleL, -m_steeringInfo.maxCorrectionAngle, m_steeringInfo.maxCorrectionAngle);
            m_ackermannAngleR = Mathf.Clamp(m_ackermannAngleR, -m_steeringInfo.maxCorrectionAngle, m_steeringInfo.maxCorrectionAngle);
        }
        
        public void UpdateSteering(float inputSteering, List<float> slipAngles, List<float> lateralAccelerations)
        {
            RawSteeringInputLerp(inputSteering);
            UpdateAckermann();
            UpdateCorrection(slipAngles, lateralAccelerations);
            
            m_steerAngles[0] = Mathf.Lerp(m_steerAngles[0], m_ackermannAngleR, m_steeringInfo.correctionSpeed * Time.fixedDeltaTime);
            m_steerAngles[1] = Mathf.Lerp(m_steerAngles[1], m_ackermannAngleL, m_steeringInfo.correctionSpeed * Time.fixedDeltaTime);
        }
        
        private void InitializeAckermannParams(List<Transform> wheelTransform)
        {
            m_wheelBase = Vector3.Distance(wheelTransform[0].position, wheelTransform[2].position);
            m_rearTrack = Vector3.Distance(wheelTransform[2].position, wheelTransform[3].position);  
        }
    }
}
