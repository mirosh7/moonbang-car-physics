using Car.Models;
using Car.Models.WheelModels;

namespace Car.Controllers.WheelControllers
{
    public class AccelerationWheelController : ICarController
    {
        private int m_wheelIndex;
        private AccelerationWheelModel m_accelerationWheelModel;
        private TireForceModel m_tireForceModel;
        private DifferentialModel m_differentialModel;
        private BrakesModel m_brakesModel;

        public AccelerationWheelController(int wheelIndex, AccelerationWheelModel accelerationWheelModel, TireForceModel tireForceModel, DifferentialModel differentialModel, BrakesModel brakesModel)
        {
            m_wheelIndex = wheelIndex;
            m_accelerationWheelModel = accelerationWheelModel;
            m_tireForceModel = tireForceModel;
            m_differentialModel = differentialModel;
            m_brakesModel = brakesModel;
        }

        public void OnUpdate()
        {
            UpdateWheelAcceleration();
        }

        private void UpdateWheelAcceleration()
        {
            var driveTorque = m_differentialModel.outputTorque[m_wheelIndex];
            var brakeTorque = m_brakesModel.brakeTorque[m_wheelIndex];
            m_accelerationWheelModel.UpdateWheelAcceleration(m_tireForceModel.fx, driveTorque, brakeTorque);
        }
    }
}
