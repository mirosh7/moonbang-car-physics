using System.Collections.Generic;
using Car.Models.PhysicsModels.WheelModels;
using UnityEngine;

namespace Car.Controllers.PhysicsControllers.WheelControllers
{
    public class TireForcesSystemController : ICarController
    {
        private TireForceSystemModel m_tireForceSystemModel;
        private List<Transform> m_wheelRoots;
        private RaycastWheelSystemModel m_raycastWheelSystemModel;
        private SlipForcesSystemModel m_slipForcesSystemModel;
        private SuspensionForcesSystemModel m_suspensionForcesSystemModel;

        public TireForcesSystemController(TireForceSystemModel tireForceSystemModel, List<Transform> wheelRoots, RaycastWheelSystemModel raycastWheelSystemModel, SlipForcesSystemModel slipForcesSystemModel, SuspensionForcesSystemModel suspensionForcesSystemModel)
        {
            m_tireForceSystemModel = tireForceSystemModel;
            m_wheelRoots = wheelRoots;
            m_raycastWheelSystemModel = raycastWheelSystemModel;
            m_slipForcesSystemModel = slipForcesSystemModel;
            m_suspensionForcesSystemModel = suspensionForcesSystemModel;
        }

        public void OnUpdate()
        {
            UpdateTireForces();
        }

        private void UpdateTireForces()
        {
            var raycastHits = m_raycastWheelSystemModel.raycastHits;
            var slipForces = m_slipForcesSystemModel.slipForces;
            var suspensionForces = m_suspensionForcesSystemModel.suspensionForces;
            m_tireForceSystemModel.UpdateWheelTireForces(m_wheelRoots, raycastHits, slipForces, suspensionForces, m_raycastWheelSystemModel.wheelHitStates);
        }
    }
}
