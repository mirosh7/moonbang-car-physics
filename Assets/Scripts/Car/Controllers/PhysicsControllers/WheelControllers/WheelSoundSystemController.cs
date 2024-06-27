using Car.Models.PhysicsModels.WheelModels;

namespace Car.Controllers.PhysicsControllers.WheelControllers
{
    public class WheelSoundSystemController : ICarController
    {
        private WheelSoundSystemModel m_wheelSoundSystemModel;
        private TireForceSystemModel m_tireForceSystemModel;

        public WheelSoundSystemController(WheelSoundSystemModel wheelSoundSystemModel, TireForceSystemModel tireForceSystemModel)
        {
            m_wheelSoundSystemModel = wheelSoundSystemModel;
            m_tireForceSystemModel = tireForceSystemModel;
        }

        public void OnCarUpdate()
        {
            UpdateWheelSounds();
        }

        private void UpdateWheelSounds()
        {
            m_wheelSoundSystemModel.UpdateWheelSounds(m_tireForceSystemModel.normalizedTireForceMagnitudes);
        }
    }
}
