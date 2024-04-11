using System.Collections.Generic;
using Car.Models.PhysicsModels.WheelModels;
using UnityEngine;

namespace Car.Controllers.PhysicsControllers.WheelControllers
{
    public class RaycastWheelSystemController : ICarController
    {
        private List<Transform> m_wheelRoots;
        private RaycastWheelSystemModel m_raycastWheelSystemModel;

        public RaycastWheelSystemController(List<Transform> wheelRoots, RaycastWheelSystemModel raycastWheelSystemModel)
        {
            m_wheelRoots = wheelRoots;
            m_raycastWheelSystemModel = raycastWheelSystemModel;
        }

        public void OnCarUpdate()
        {
            UpdateRaycasts();
        }

        private void UpdateRaycasts()
        {
            m_raycastWheelSystemModel.UpdateWheelsRaycast(m_wheelRoots);
        }
    }
}
