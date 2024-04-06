using System.Collections.Generic;
using Car.Data;
using Car.Models.WheelComponents;
using UnityEngine;

namespace Car.Models.WheelModels
{
    public class TireForceSystemModel
    {
        private List<TireForceComponent> m_tireForceComponents = new List<TireForceComponent>();
        private List<float> m_fxVelocities = new List<float>();

        public List<float> fxVelocities => m_fxVelocities;

        public TireForceSystemModel(List<CarDesc.WheelInfo> wheelInfos, Rigidbody rb)
        {
            foreach (var wheelInfo in wheelInfos)
            {
                m_tireForceComponents.Add(new TireForceComponent(wheelInfo, rb));
            }
            
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
