using System.Collections.Generic;
using Car;
using Car.Data;
using UI.Debug;
using UnityEngine;
using UnityEngine.Serialization;

public class RaceCarState : MonoBehaviour
{
    private List<IController> m_controllers = new List<IController>();

    private InputManager m_inputManager;
    
    [SerializeField]
    private CarDesc m_carDesc;
    
    [SerializeField]
    private Transform m_carSpawnPoint;
    
    [SerializeField]
    private UICarPhysicsInfo m_carPhysicsInfo;
    [SerializeField]
    private UICarInputInfo m_carInputInfo;

    private RaceCar m_currentCar;
    private CarBuilder m_carBuilder;

    private CarPhysicsInfoController m_carPhysicsInfoController;
    private CarInputInfoController m_carInputInfoController;
    
    private void Start()
    {
        m_inputManager = InputManager.instance;
        m_carBuilder = new CarBuilder(m_carDesc, "Mazda", "Wheel");
        ResetCar();
        CreateUIControllers();
    }

    private void Update()
    {
        foreach (var controller in m_controllers)
        {
            controller.OnUpdate();
        }
    }

    private void ResetCar()
    {
        m_currentCar = m_carBuilder.BuildCar(m_carSpawnPoint);
    }

    private void CreateUIControllers()
    {
        m_carPhysicsInfoController = new CarPhysicsInfoController(m_carPhysicsInfo,
            m_carBuilder.engineModel,
            m_carBuilder.clutchModel,
            m_carBuilder.gearShiftingModel);
        m_controllers.Add(m_carPhysicsInfoController);

        m_carInputInfoController = new CarInputInfoController(m_carInputInfo, m_inputManager);
        m_controllers.Add(m_carInputInfoController);
    }
}