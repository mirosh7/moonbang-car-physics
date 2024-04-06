using System.Collections.Generic;
using Car.Models.WheelComponents;
using UnityEngine;

namespace Car.Models.WheelModels
{
    public class VisualWheelSystemModel
    {
        private List<VisualWheelComponent> m_visualWheelComponents;
        
        public VisualWheelSystemModel(List<VisualWheelComponent> visualWheelComponents)
        {
            m_visualWheelComponents = visualWheelComponents;
        } 

        public void UpdateWheelsVisual(List<Transform> wheelVisuals, List<Transform> wheelRoots, List<float> angularVelocities, List<float> currentLengths, float[] steerAngles)
        {
            for (int i = 0; i < m_visualWheelComponents.Count; i++)
            {
                m_visualWheelComponents[i].ApplyVisuals(wheelVisuals[i], wheelRoots[i], angularVelocities[i], currentLengths[i], steerAngles[i]);
            }
        }
    }
}
