using System.Collections.Generic;
using Car.Data;
using UnityEngine;

namespace Car.Models.PhysicsModels
{
    public class BrakesModel
    {
        private CarDesc.BrakesInfo m_brakesInfo;
        private List<float> m_brakeTorque = new List<float>(4);
        
        public List<float> brakeTorque => m_brakeTorque;

        public BrakesModel(CarDesc.BrakesInfo brakesInfo)
        {
            m_brakesInfo = brakesInfo;
            
            for (int i = 0; i < 4; i++)
            {
                m_brakeTorque.Add(0f);
            }
        }

        public void UpdateBrakes(float brakeInput, List<float> angularVelocities)
        {
            m_brakeTorque[0] = brakeInput * m_brakesInfo.brakeBias[0] *  m_brakesInfo.maxTorque *  m_brakesInfo.brakeTorqueCurve.Evaluate(Mathf.Abs((angularVelocities[0] + angularVelocities[2]) * 0.5f));
            m_brakeTorque[1] = m_brakeTorque[0];
            m_brakeTorque[2] = brakeInput *  m_brakesInfo.brakeBias[1] *  m_brakesInfo.maxTorque *  m_brakesInfo.brakeTorqueCurve.Evaluate(Mathf.Abs((angularVelocities[1] + angularVelocities[3]) * 0.5f));
            m_brakeTorque[3] = m_brakeTorque[2];
        }
    }
}
