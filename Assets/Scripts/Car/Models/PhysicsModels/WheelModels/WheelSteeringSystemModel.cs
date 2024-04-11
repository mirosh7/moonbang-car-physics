using System.Collections.Generic;
using Car.Data;
using Car.Models.PhysicsModels.WheelComponents;
using UnityEngine;

namespace Car.Models.PhysicsModels.WheelModels
{
    public class WheelSteeringSystemModel
    {
        private List<WheelSteeringComponent> m_wheelSteeringComponents = new List<WheelSteeringComponent>();
        
        public WheelSteeringSystemModel(List<Transform> wheelRoots)
        {
            foreach (var wheelRoot in wheelRoots)
            {
                m_wheelSteeringComponents.Add(new WheelSteeringComponent(wheelRoot));
            }
        }

        public void UpdateWheelRootSteering(List<float> steerAngles)
        {
            for (int i = 0; i < m_wheelSteeringComponents.Count; i++)
            {
                m_wheelSteeringComponents[i].UpdateWheelSteer(steerAngles[i]);
            }
        }
    }
}
