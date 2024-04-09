using System.Collections.Generic;
using Car.Data;
using Car.Models.PhysicsModels.WheelComponents;
using UnityEngine;

namespace Car.Models.PhysicsModels.WheelModels
{
    public class RaycastWheelSystemModel
    {
        private List<RaycastWheelComponent> m_raycastWheelComponents = new List<RaycastWheelComponent>();
        private List<RaycastHit> m_raycastHits = new List<RaycastHit>();
        private List<bool> m_wheelHits = new List<bool>();
        
        public List<RaycastHit> raycastHits => m_raycastHits;
        public List<bool> wheelHitStates => m_wheelHits;

        public RaycastWheelSystemModel(List<CarDesc.WheelInfo> wheelInfos)
        {
            foreach (var wheelInfo in wheelInfos)
            {
                m_raycastWheelComponents.Add(new RaycastWheelComponent(wheelInfo));
            }
            
            foreach (var raycastWheelComponent in m_raycastWheelComponents)
            {
                m_raycastHits.Add(raycastWheelComponent.wheelHit);
                m_wheelHits.Add(raycastWheelComponent.isWheelHit);
            }
        } 

        public void UpdateWheelsRaycast(List<Transform> wheelRoots)
        {
            for (int i = 0; i < m_raycastWheelComponents.Count; i++)
            {
                m_raycastWheelComponents[i].UpdateRaycast(wheelRoots[i]);
                m_raycastHits[i] = m_raycastWheelComponents[i].wheelHit;
                m_wheelHits[i] = m_raycastWheelComponents[i].isWheelHit;
            }
        }
    }
}
