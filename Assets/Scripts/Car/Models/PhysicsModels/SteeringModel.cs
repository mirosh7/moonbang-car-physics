using System.Collections.Generic;
using System.Linq;
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
        private float m_carSteerInput;
        private float m_maxCorrectionAngle;
        private float m_correctionSpeed;

        public SteeringModel(CarDesc.SteeringInfo steeringInfo, List<Transform> wheelTransform)
        {
            m_turnRadius = steeringInfo.turnRadius;
            m_steerForce = steeringInfo.steerForce;
            m_maxCorrectionAngle = steeringInfo.maxCorrectionAngle;
            m_correctionSpeed = steeringInfo.correctionSpeed;
            
            foreach (var wheel in wheelTransform)
            {
                m_steerAngles.Add(0f);
            }
            
            InitializeAckermannParams(wheelTransform);
        }

        public List<float> steerAngles => m_steerAngles;

        private void UpdateAckermann()
        {
            if (m_carSteerInput > 0)
            {
                m_ackermannAngleL = Mathf.Rad2Deg * Mathf.Atan(m_wheelBase / (m_turnRadius + m_rearTrack / 2)) * m_carSteerInput * m_steerForce;
                m_ackermannAngleR = Mathf.Rad2Deg * Mathf.Atan(m_wheelBase / (m_turnRadius - m_rearTrack / 2)) * m_carSteerInput * m_steerForce;
            }
            else if (m_carSteerInput < 0)
            {
                m_ackermannAngleL = Mathf.Rad2Deg * Mathf.Atan(m_wheelBase / (m_turnRadius - m_rearTrack / 2)) * m_carSteerInput * m_steerForce;
                m_ackermannAngleR = Mathf.Rad2Deg * Mathf.Atan(m_wheelBase / (m_turnRadius + m_rearTrack / 2)) * m_carSteerInput * m_steerForce;
            }
        }

        private void RawSteeringInputLerp(float inputSteering)
        {
            m_carSteerInput = Mathf.Lerp(m_carSteerInput, inputSteering, Time.fixedDeltaTime);
        }

        private void UpdateCorrection(List<float> slipAngles)
        {
            if (m_carSteerInput != 0)
            {
                return;
            }
            
            float correctionFactor = m_correctionSpeed * Time.fixedDeltaTime;
            m_ackermannAngleL -= slipAngles[1] * correctionFactor;
            m_ackermannAngleR -= slipAngles[0] * correctionFactor;
                
            m_ackermannAngleL = Mathf.Clamp(m_ackermannAngleL, -m_maxCorrectionAngle, m_maxCorrectionAngle);
            m_ackermannAngleR = Mathf.Clamp(m_ackermannAngleR, -m_maxCorrectionAngle, m_maxCorrectionAngle);
        }
        
        public void UpdateSteering(float inputSteering, List<float> slipAngles, List<float> lateralAccelerations)
        {
            RawSteeringInputLerp(inputSteering);
            UpdateAckermann();
            UpdateCorrection(slipAngles);
            
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
