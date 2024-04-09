using System.Collections.Generic;
using Car.Models.PhysicsModels.WheelModels;
using UnityEngine;

namespace Car.Controllers.PhysicsControllers.WheelControllers
{
    public class SuspensionForcesSystemController : ICarController
    {
        private SuspensionForcesSystemModel m_suspensionForcesSystemModel;
        private RaycastWheelSystemModel m_raycastWheelSystemModel;
        private List<Transform> m_wheelRoots;

        public SuspensionForcesSystemController(SuspensionForcesSystemModel suspensionForcesSystemModel, RaycastWheelSystemModel raycastWheelSystemModel, List<Transform> wheelRoots)
        {
            m_suspensionForcesSystemModel = suspensionForcesSystemModel;
            m_raycastWheelSystemModel = raycastWheelSystemModel;
            m_wheelRoots = wheelRoots;
        }

        public void OnUpdate()
        {
            UpdateSuspensionForces();
        }

        private void UpdateSuspensionForces()
        {
            m_suspensionForcesSystemModel.UpdateWheelsSuspension(m_raycastWheelSystemModel.raycastHits, m_wheelRoots, m_raycastWheelSystemModel.wheelHitStates);
        }
    }
}
