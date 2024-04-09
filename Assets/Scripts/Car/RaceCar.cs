using System;
using System.Collections.Generic;
using Car.Controllers.PhysicsControllers;
using Car.Data;
using UnityEngine;

namespace Car
{
    public class RaceCar : MonoBehaviour
    {
        private List<ICarController> m_controllers;

        public void SetControllers(List<ICarController> controllers)
        {
            m_controllers = controllers;
        }
        
        private void FixedUpdate()
        {
            if (m_controllers == null)
            {
                return;
            }
            foreach (var controller in m_controllers)
            {
                controller.OnUpdate();
            }
        }
    }
}
