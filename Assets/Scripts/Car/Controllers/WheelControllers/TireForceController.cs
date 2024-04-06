using Car.Models.WheelModels;
using UnityEngine;

namespace Car.Controllers.WheelControllers
{
    public class TireForceController : ICarController
    {
        private int m_wheelIndex;
        private Rigidbody m_rb;
        private Transform m_wheelRoot;
        private TireForceModel m_tireForceModel;
        private RaycastWheelModel m_raycastWheelModel;
        private SuspensionForceModel m_suspensionForceModel;
        private SlipForceModel m_slipForceModel;
        private AccelerationWheelSystemModel m_accelerationWheelSystemModel;

        public TireForceController(int wheelIndex, Rigidbody rb, Transform wheelRoot, TireForceModel tireForceModel, RaycastWheelModel raycastWheelModel, SuspensionForceModel suspensionForceModel, SlipForceModel slipForceModel, AccelerationWheelSystemModel accelerationWheelSystemModel)
        {
            m_wheelIndex = wheelIndex;
            m_rb = rb;
            m_wheelRoot = wheelRoot;
            m_tireForceModel = tireForceModel;
            m_raycastWheelModel = raycastWheelModel;
            m_suspensionForceModel = suspensionForceModel;
            m_slipForceModel = slipForceModel;
            m_accelerationWheelSystemModel = accelerationWheelSystemModel;
        }

        public void OnUpdate()
        {
           UpdateTireForce();
        }

        private void UpdateTireForce()
        {
            if (!m_raycastWheelModel.isWheelHit)
            {
                return;
            }
            
            var linearVelocity = m_suspensionForceModel.linearVelocity;
            var suspensionForce = m_suspensionForceModel.suspensionForce;
            var angularVelocity = m_accelerationWheelSystemModel.angularVelocities[m_wheelIndex];
            var longidudalForce = m_slipForceModel.GetLongitudinalForce(linearVelocity, suspensionForce, angularVelocity);
            var lateralForce = m_slipForceModel.GetLateralForce(linearVelocity);
            m_tireForceModel.UpdateTireForce(m_rb, m_wheelRoot, m_raycastWheelModel.wheelHit, longidudalForce, lateralForce, suspensionForce);
        }
    }
}
