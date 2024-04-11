using Car.Models.PhysicsModels;
using Car.Models.PhysicsModels.WheelModels;

namespace Car.Controllers.PhysicsControllers.WheelControllers
{
    public class WheelSteeringSystemController : ICarController
    {
        private WheelSteeringSystemModel m_steeringSystemModel;
        private SteeringModel m_steeringModel;

        public WheelSteeringSystemController(WheelSteeringSystemModel steeringSystemModel, SteeringModel steeringModel)
        {
            m_steeringSystemModel = steeringSystemModel;
            m_steeringModel = steeringModel;
        }

        public void OnCarUpdate()
        {
            UpdateWheelSteering();
        }

        private void UpdateWheelSteering()
        {
            m_steeringSystemModel.UpdateWheelRootSteering(m_steeringModel.steerAngles);
        }
    }
}
