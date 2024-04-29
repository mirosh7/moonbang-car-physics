using Car.Models.PhysicsModels.WheelModels;

namespace Car.Controllers.PhysicsControllers.WheelControllers
{
    public class WheelSoundSystemController : ICarController
    {
        private WheelSoundSystemModel m_wheelSoundSystemModel;
        private SlipForcesSystemModel m_slipForcesSystemModel;

        public WheelSoundSystemController(WheelSoundSystemModel wheelSoundSystemModel, SlipForcesSystemModel slipForcesSystemModel)
        {
            m_wheelSoundSystemModel = wheelSoundSystemModel;
            m_slipForcesSystemModel = slipForcesSystemModel;
        }

        public void OnCarUpdate()
        {
            UpdateWheelSounds();
        }

        private void UpdateWheelSounds()
        {
            m_wheelSoundSystemModel.UpdateWheelSounds(m_slipForcesSystemModel.slipAngles);
        }
    }
}
