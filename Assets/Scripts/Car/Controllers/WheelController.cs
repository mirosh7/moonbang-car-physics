using Car.Models.WheelModels;

namespace Car.Controllers
{
    public class WheelController : ICarController
    {
        private RaycastWheelModel m_raycastWheelModel;
        private VisualWheelModel m_visualWheelModel;
        private SuspensionForceModel m_suspensionForceModel;
        
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
            
            
            
        }
    }
}
