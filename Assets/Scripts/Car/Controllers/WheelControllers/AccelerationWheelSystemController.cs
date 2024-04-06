using Car.Models;
using Car.Models.WheelModels;

namespace Car.Controllers.WheelControllers
{
    public class AccelerationWheelSystemController : ICarController
    {
        private AccelerationWheelSystemModel m_accelerationWheelModel;
        private RaycastWheelModel m_raycastWheelModel;

        public AccelerationWheelSystemController(AccelerationWheelSystemModel accelerationWheelModel, RaycastWheelModel raycastWheelModel)
        {
            m_raycastWheelModel = raycastWheelModel;
            m_accelerationWheelModel = accelerationWheelModel;
        }

        public void OnUpdate()
        {
            UpdateWheelsAcceleration();
        }

        private void UpdateWheelsAcceleration()
        {
            if (!m_raycastWheelModel.isWheelHit)
            {
                return;
            }
            
            m_accelerationWheelModel.UpdateWheelsAcceleration();
        }
    }
}
