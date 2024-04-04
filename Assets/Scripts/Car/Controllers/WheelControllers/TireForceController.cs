using Car.Models.WheelModels;
using UnityEngine;

namespace Car.Controllers.WheelControllers
{
    public class TireForceController : ICarController
    {
        private Rigidbody m_rb;
        private Transform m_wheelRoot;
        private TireForceModel m_tireForceModel;
        private RaycastWheelModel m_raycastWheelModel;
        private SuspensionForceModel m_suspensionForceModel;
        private SlipForceModel m_slipForceModel;
        private AccelerationWheelModel m_accelerationWheelModel;

        public TireForceController(Rigidbody rb, Transform wheelRoot, TireForceModel tireForceModel, RaycastWheelModel raycastWheelModel, SuspensionForceModel suspensionForceModel, SlipForceModel slipForceModel, AccelerationWheelModel accelerationWheelModel)
        {
            m_rb = rb;
            m_wheelRoot = wheelRoot;
            m_tireForceModel = tireForceModel;
            m_raycastWheelModel = raycastWheelModel;
            m_suspensionForceModel = suspensionForceModel;
            m_slipForceModel = slipForceModel;
            m_accelerationWheelModel = accelerationWheelModel;
        }

        public void OnUpdate()
        {
           UpdateTireForce();
        }

        private void UpdateTireForce()
        {
            var linearVelocity = m_suspensionForceModel.linearVelocity;
            var suspensionForce = m_suspensionForceModel.suspensionForce;
            var angularVelocity = m_accelerationWheelModel.angularVelocity;
            var longidudalForce = m_slipForceModel.GetLongitudinalForce(linearVelocity, suspensionForce, angularVelocity);
            var lateralForce = m_slipForceModel.GetLateralForce(linearVelocity);
            m_tireForceModel.UpdateTireForce(m_rb, m_wheelRoot, m_raycastWheelModel.wheelHit, longidudalForce, lateralForce, suspensionForce);
        }
    }
}
