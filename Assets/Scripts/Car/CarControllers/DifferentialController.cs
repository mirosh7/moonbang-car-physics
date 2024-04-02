using System.Collections.Generic;
using Car.Core;
using UnityEngine;

namespace Car.CarControllers
{
    public class DifferentialController : ICarController
    {
        private List<WheelController> m_wheels = new List<WheelController>(4);
        private float m_differentialRatio;
        private bool m_isDiffLocked;
        private float[] m_outputTorque = new float[2];
        private float m_inputShaftVelocity;
        private float m_angularVelocityL;
        private float m_angularVelocityR;
    
    
        public void OnUpdatePhysics()
        {
            throw new System.NotImplementedException();
        }
    
        public float[] GetOutputTorque(float inputTorque)
        {
            if (m_isDiffLocked)
            {
                m_angularVelocityL = m_wheels[2].angularVelocity;
                m_angularVelocityR = m_wheels[3].angularVelocity;
            
                var vel = (m_angularVelocityL - m_angularVelocityR) * 0.5f / Time.fixedDeltaTime * m_wheels[0].wheelInertia;
            
                m_outputTorque[0] = (inputTorque * 0.5f * m_differentialRatio) - vel;
                m_outputTorque[1] = (inputTorque * 0.5f * m_differentialRatio) + vel;
            
                return m_outputTorque;
            }
            else
            {
                m_outputTorque[0] = inputTorque * m_differentialRatio * 0.5f;
                m_outputTorque[1] = inputTorque * m_differentialRatio * 0.5f;
            
                return m_outputTorque;
            }
        }
    
        public float GetInputShaftVelocity()
        {
            float inputShaftVelocity = (m_angularVelocityL + m_angularVelocityR) * 0.5f * m_differentialRatio;
            return inputShaftVelocity;
        }
    }
}
