using Car.Models;
using Car.Models.WheelModels;
using UnityEngine;

namespace Car.Controllers.WheelControllers
{
    public class VisualWheelController : ICarController
    {
        private int m_wheelIndex;
        private VisualWheelModel m_visualWheelModel;
        private AccelerationWheelSystemModel m_accelerationWheelSystemModel;
        private SuspensionForceModel m_suspensionForceModel;
        private SteeringModel m_steeringModel;
        private Transform m_wheelVisual;
        private Transform m_wheelRoot;

        public VisualWheelController(int wheelIndex, VisualWheelModel visualWheelModel, AccelerationWheelSystemModel accelerationWheelSystemModel, SuspensionForceModel suspensionForceModel, SteeringModel steeringModel, Transform wheelVisual, Transform wheelRoot)
        {
            m_wheelIndex = wheelIndex;
            m_visualWheelModel = visualWheelModel;
            m_accelerationWheelSystemModel = accelerationWheelSystemModel;
            m_suspensionForceModel = suspensionForceModel;
            m_steeringModel = steeringModel;
            m_wheelVisual = wheelVisual;
            m_wheelRoot = wheelRoot;
        }

        public void OnUpdate()
        {
            UpdateVisual();
        }

        private void UpdateVisual()
        {
            var steerAngle = m_steeringModel.steerAngle[m_wheelIndex];
            m_visualWheelModel.ApplyVisuals(m_wheelVisual, m_wheelRoot, m_accelerationWheelSystemModel.angularVelocities[m_wheelIndex], m_suspensionForceModel.currentLength, steerAngle);
        }
    }
}
