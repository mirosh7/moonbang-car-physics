using System.Collections.Generic;
using Car.Models.PhysicsModels;
using Car.Models.PhysicsModels.WheelModels;
using UnityEngine;

namespace Car.Controllers.PhysicsControllers.WheelControllers
{
    public class VisualWheelSystemController : ICarController
    {
        private AccelerationWheelSystemModel m_accelerationWheelSystemModel;
        private VisualWheelSystemModel m_visualWheelSystemModel;
        private SuspensionForcesSystemModel m_suspensionForcesSystemModel;
        private SteeringModel m_steeringModel;
        private List<Transform> m_wheelVisuals;
        private List<Transform> m_wheelRoots;

        public VisualWheelSystemController(VisualWheelSystemModel visualWheelSystemModel, AccelerationWheelSystemModel accelerationWheelSystemModel, SuspensionForcesSystemModel suspensionForcesSystemModel, SteeringModel steeringModel, List<Transform> wheelVisuals, List<Transform> wheelRoots)
        {
            m_visualWheelSystemModel = visualWheelSystemModel;
            m_accelerationWheelSystemModel = accelerationWheelSystemModel;
            m_suspensionForcesSystemModel = suspensionForcesSystemModel;
            m_steeringModel = steeringModel;
            m_wheelVisuals = wheelVisuals;
            m_wheelRoots = wheelRoots;
        }

        public void OnUpdate()
        {
            UpdateVisual();
        }

        private void UpdateVisual()
        {
            m_visualWheelSystemModel.UpdateWheelsVisual(m_wheelVisuals, m_wheelRoots, m_accelerationWheelSystemModel.angularVelocities, m_suspensionForcesSystemModel.currentLengths, m_steeringModel.steerAngle);
        }
    }
}
