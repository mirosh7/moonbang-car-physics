using System.Collections.Generic;
using Car.Data;
using Car.Models.WheelComponents;
using UnityEngine;

namespace Car.Models.WheelModels
{
    public class SlipForcesSystemModel
    {
        private List<SlipForceComponent> m_slipForceComponents = new List<SlipForceComponent>();
        private List<Vector2> m_slipForces = new List<Vector2>();

        public List<Vector2> slipForces => m_slipForces;

        public SlipForcesSystemModel(List<CarDesc.WheelInfo> wheelInfos)
        {
            foreach (var wheelInfo in wheelInfos)
            {
                m_slipForceComponents.Add(new SlipForceComponent(wheelInfo));
            }
           
            foreach (var slipForceComponent in m_slipForceComponents)
            {
                m_slipForces.Add(slipForceComponent.slipForce);
            }
        } 

        public void UpdateSlipForces(List<Vector3> linearVelocities, List<float> suspensionForces, List<float> angularVelocities)
        {
            for (int i = 0; i <= m_slipForceComponents.Count; i++)
            {
                m_slipForceComponents[i].UpdateSlipForces(linearVelocities[i], suspensionForces[i], angularVelocities[i]);
                m_slipForces[i] = m_slipForceComponents[i].slipForce;
            }
        }
    }
}
