using Car.Models.WheelModels;
using UnityEngine;

namespace Car.Controllers.WheelControllers
{
    public class VisualWheelController : ICarController
    {
        private VisualWheelModel m_visualWheelModel;
        private AccelerationWheelModel m_accelerationWheelModel;
        private SuspensionForceModel m_suspensionForceModel;
        private Transform m_wheelVisual;
        private Transform m_wheelRoot;
        
        public void OnUpdate()
        {
            throw new System.NotImplementedException();
        }

        private void UpdateVisual()
        {
            m_visualWheelModel.ApplyVisuals(m_wheelVisual, m_wheelRoot, m_accelerationWheelModel.angularVelocity, m_suspensionForceModel.currentLength, );
        }
    }
}
