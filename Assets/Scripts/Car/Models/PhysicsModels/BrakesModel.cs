using System.Collections.Generic;
using Car.Data;
using UnityEngine;

namespace Car.Models.PhysicsModels
{
    public class BrakesModel
    {
        private List<float> m_brakeTorque = new List<float>(4);
        private List<float> m_brakeBias = new List<float>(4);
        private AnimationCurve m_brakeTorqueCurve;
        private float m_maxTorque;

        public List<float> brakeTorque => m_brakeTorque;

        public BrakesModel(CarDesc.BrakesInfo brakesInfo)
        {
            m_brakeTorqueCurve = brakesInfo.brakeTorqueCurve;
            m_maxTorque = brakesInfo.maxTorque;
            m_brakeBias = brakesInfo.brakeBias;
            
            for (int i = 0; i < 4; i++)
            {
                m_brakeTorque.Add(0f);
            }
        }

        public void UpdateBrakes(float brakeInput, List<float> angularVelocities)
        {
            m_brakeTorque[0] = -brakeInput * m_brakeBias[0] * m_maxTorque * m_brakeTorqueCurve.Evaluate(Mathf.Abs((angularVelocities[0] + angularVelocities[2]) * 0.5f));
            m_brakeTorque[1] = -brakeInput * m_brakeBias[1] * m_maxTorque * m_brakeTorqueCurve.Evaluate(Mathf.Abs((angularVelocities[1] + angularVelocities[3]) * 0.5f));
        }
    }
}
