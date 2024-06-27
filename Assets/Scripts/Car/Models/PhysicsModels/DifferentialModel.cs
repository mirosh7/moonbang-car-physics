using System.Collections.Generic;
using Car.Data;
using UnityEngine;

namespace Car.Models.PhysicsModels
{
    public class DifferentialModel
    {
        private CarDesc.DifferentialInfo m_differentialInfo;
        private CarDesc.WheelInfo m_wheelInfo;
        private List<float> m_outputTorque = new List<float>(4);
        private float m_inputShaftVelocity;
        private float m_angularVelocityL;
        private float m_angularVelocityR;
        
        public List<float> outputTorque => m_outputTorque;

        public DifferentialModel(CarDesc.DifferentialInfo differentialInfo, CarDesc.WheelInfo wheelInfo)
        {
            m_differentialInfo = differentialInfo;
            m_wheelInfo = wheelInfo;
            
            for (int i = 0; i < 4; i++)
            {
                m_outputTorque.Add(0f);
            }
        }
        
        public void UpdateOutputTorque(float inputTorque, List<float> wheelAngularVelocities)
        {
            m_angularVelocityL = wheelAngularVelocities[2];
            m_angularVelocityR = wheelAngularVelocities[3];
            
            if (!m_differentialInfo.isDiffLocked)
            {
                var vel = (m_angularVelocityL - m_angularVelocityR) * 0.5f / Time.fixedDeltaTime * m_wheelInfo.wheelInertia;
            
                m_outputTorque[0] = (inputTorque * 0.5f * m_differentialInfo.differentialRatio) - vel;
                m_outputTorque[1] = (inputTorque * 0.5f * m_differentialInfo.differentialRatio) + vel;
                m_outputTorque[2] = (inputTorque * 0.5f * m_differentialInfo.differentialRatio) - vel;
                m_outputTorque[3] = (inputTorque * 0.5f * m_differentialInfo.differentialRatio) + vel;
            }
            else
            {
                m_outputTorque[0] = inputTorque * m_differentialInfo.differentialRatio * 0.5f;
                m_outputTorque[1] = inputTorque * m_differentialInfo.differentialRatio * 0.5f;
                m_outputTorque[2] = inputTorque * m_differentialInfo.differentialRatio * 0.5f;
                m_outputTorque[3] = inputTorque * m_differentialInfo.differentialRatio * 0.5f;
            }
        }
    
        public float GetInputShaftVelocity()
        {
            float inputShaftVelocity = (m_angularVelocityL + m_angularVelocityR) * 0.5f *  m_differentialInfo.differentialRatio;
            return inputShaftVelocity;
        }
    }
}
