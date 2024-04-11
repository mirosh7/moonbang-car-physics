using System.Collections.Generic;
using Car.Data;
using UnityEngine;

namespace Car.Models.PhysicsModels
{
    public class DifferentialModel
    {
        private float m_differentialRatio;
        private bool m_isDiffLocked;
        private List<float> m_outputTorque = new List<float>(4);
        private float m_inputShaftVelocity;
        private float m_angularVelocityL;
        private float m_angularVelocityR;
        private float m_wheelInertia;
        
        public List<float> outputTorque => m_outputTorque;

        public DifferentialModel(CarDesc.DifferentialInfo differentialInfo, CarDesc.WheelInfo wheelInfo)
        {
            m_differentialRatio = differentialInfo.differentialRatio;
            m_isDiffLocked = differentialInfo.isDiffLocked;
            m_wheelInertia = wheelInfo.wheelInertia;

            for (int i = 0; i < 4; i++)
            {
                m_outputTorque.Add(0f);
            }
        }
        
        public void UpdateOutputTorque(float inputTorque, List<float> wheelAngularVelocities)
        {
            if (m_isDiffLocked)
            {
                m_angularVelocityL = wheelAngularVelocities[2];
                m_angularVelocityR = wheelAngularVelocities[3];
            
                var vel = (m_angularVelocityL - m_angularVelocityR) * 0.5f / Time.fixedDeltaTime * m_wheelInertia;
            
                m_outputTorque[2] = (inputTorque * 0.5f * m_differentialRatio) - vel;
                m_outputTorque[3] = (inputTorque * 0.5f * m_differentialRatio) + vel;
            }
            else
            {
                m_outputTorque[2] = inputTorque * m_differentialRatio * 0.5f;
                m_outputTorque[3] = inputTorque * m_differentialRatio * 0.5f;
            }
        }
    
        public float GetInputShaftVelocity()
        {
            float inputShaftVelocity = (m_angularVelocityL + m_angularVelocityR) * 0.5f * m_differentialRatio;
            return inputShaftVelocity;
        }
    }
}
