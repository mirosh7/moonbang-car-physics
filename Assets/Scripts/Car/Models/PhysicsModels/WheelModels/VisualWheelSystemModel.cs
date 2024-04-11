using System.Collections.Generic;
using Car.Data;
using Car.Models.PhysicsModels.WheelComponents;
using UnityEngine;

namespace Car.Models.PhysicsModels.WheelModels
{
    public class VisualWheelSystemModel
    {
        private List<VisualWheelComponent> m_visualWheelComponents = new List<VisualWheelComponent>();
        
        public VisualWheelSystemModel(List<CarDesc.WheelInfo> wheelInfos)
        {
            foreach (var wheelInfo in wheelInfos)
            {
                m_visualWheelComponents.Add(new VisualWheelComponent(wheelInfo));
            }
        } 

        public void UpdateWheelsVisual(List<Transform> wheelVisuals, List<Transform> wheelRoots, List<float> angularVelocities, List<float> currentLengths, List<float> steerAngles)
        {
            for (int i = 0; i < m_visualWheelComponents.Count; i++)
            {
                m_visualWheelComponents[i].ApplyVisuals(wheelVisuals[i], wheelRoots[i], angularVelocities[i], currentLengths[i], steerAngles[i], i % 2 == 0);
            }
        }
    }
}
