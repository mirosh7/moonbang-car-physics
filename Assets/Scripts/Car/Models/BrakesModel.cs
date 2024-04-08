using System.Collections.Generic;
using Car.Data;
using UnityEngine;

namespace Car.Models
{
    public class BrakesModel
    {
        private float[] m_brakeTorque = new float[2];
        private float[] m_brakeBias = new float[2];
        private AnimationCurve m_brakeTorqueCurve;
        private float m_maxTorque;

        public float[] brakeTorque => m_brakeTorque;

        public BrakesModel(CarDesc.BrakesInfo brakesInfo)
        {
            m_brakeTorqueCurve = brakesInfo.brakeTorqueCurve;
            m_maxTorque = brakesInfo.maxTorque;
        }

        public void UpdateBrakes(float brakeInput, List<float> angularVelocities)
        {
            m_brakeTorque[0] = -brakeInput * m_brakeBias[0] * m_maxTorque * m_brakeTorqueCurve.Evaluate(Mathf.Abs((angularVelocities[0] + angularVelocities[2]) * 0.5f));
            m_brakeTorque[1] = -brakeInput * m_brakeBias[1] * m_maxTorque * m_brakeTorqueCurve.Evaluate(Mathf.Abs((angularVelocities[1] + angularVelocities[3]) * 0.5f));
        }
    }
}
