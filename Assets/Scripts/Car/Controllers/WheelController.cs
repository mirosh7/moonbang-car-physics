using Car.Models.WheelModels;
using UnityEngine;

namespace Car.Controllers
{
    public class WheelController : ICarController
    {
        private RaycastWheelModel m_raycastWheelModel;
        private VisualWheelModel m_visualWheelModel;
        private SuspensionForceModel m_suspensionForceModel;
        private Transform m_wheelRoot;
        public void OnUpdate()
        {
            UpdateWheel();
        }

        private void UpdateWheel()
        {
            if (!m_raycastWheelModel.isWheelHit)
            {
                return;
            }
            m_suspensionForceModel.ApplySuspensionForce(m_raycastWheelModel.wheelHit, m_wheelRoot);
            
            
        }
    }
}
