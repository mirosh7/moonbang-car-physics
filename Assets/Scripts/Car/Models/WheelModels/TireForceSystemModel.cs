using System.Collections.Generic;
using Car.Models.WheelComponents;
using UnityEngine;

namespace Car.Models.WheelModels
{
    public class TireForceSystemModel
    {
        private Rigidbody m_rb;
        private List<TireForceComponent> m_tireForceComponents;
        private List<float> m_fxVelocities = new List<float>();

        public List<float> fxVelocities => m_fxVelocities;

        public TireForceSystemModel(List<TireForceComponent> tireForceComponents, Rigidbody rb)
        {
            m_rb = rb;
            m_tireForceComponents = tireForceComponents;
            
            foreach (var tireForceComponent in m_tireForceComponents)
            {
                m_fxVelocities.Add(tireForceComponent.fx);
            }
        }

        public void UpdateWheelTireForces(List<Transform> wheelRoots, List<RaycastHit> raycastHits, List<Vector2> slipForces, List<float> suspensionForces)
        {
            for (int i = 0; i <= m_tireForceComponents.Count; i++)
            {
                m_tireForceComponents[i].UpdateTireForce(
                    m_rb,
                    wheelRoots[i],
                    raycastHits[i],
                    slipForces[i].x,
                    slipForces[i].y,
                    suspensionForces[i]
                    );

                m_fxVelocities[i] = m_tireForceComponents[i].fx;
            }
        }

    }
}
