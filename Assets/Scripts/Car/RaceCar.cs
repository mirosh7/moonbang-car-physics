using System.Collections.Generic;
using Car.CarControllers;
using UnityEngine;

namespace Car
{
    public class RaceCar : MonoBehaviour
    {
        private InputManager m_input;
        private Rigidbody m_carRigidBody;
        private List<WheelController> m_wheels = new List<WheelController>(4);
        private EngineController m_engineController;
        private SteeringController m_steeringController;
        private GearboxController m_gearboxController;
        private ClutchController m_clutchController;
        private DifferentialController m_differentialController;
        private BrakesController m_brakesController;
        private AntiRollBarController m_antiRollBarController;

        private void Start()
        {
            m_differentialController = new DifferentialController();
            m_gearboxController = new GearboxController(this, m_differentialController);
            m_engineController = new EngineController(m_carRigidBody, m_gearboxController, m_clutchController, m_input);
        }
    }
}
