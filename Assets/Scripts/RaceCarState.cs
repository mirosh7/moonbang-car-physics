using System.Collections.Generic;
using Camera.Controllers;
using Camera.Models;
using Car;
using Car.Data;
using Cinemachine;
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
    private CinemachineFreeLook m_carCamera;
    
    [SerializeField]
    private UICarPhysicsInfo m_carPhysicsInfo;
    [SerializeField]
    private UICarInputInfo m_carInputInfo;

    private RaceCar m_currentCar;
    private CarBuilder m_carBuilder;

    private FreeCameraModel m_freeCameraModel;

    private CarPhysicsInfoController m_carPhysicsInfoController;
    private CarInputInfoController m_carInputInfoController;
    private FreeCameraController m_freeCameraController;

    public CarBuilder carBuilder => m_carBuilder;
    
    private void Start()
    {
        m_inputManager = InputManager.instance;
        m_carBuilder = new CarBuilder(m_carDesc, "porsche", "porscheWheel");
        ResetCar();
        CreateModels();
        CreateControllers();
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

    private void CreateModels()
    {
        m_freeCameraModel = new FreeCameraModel(m_carCamera, m_inputManager);
    }

    private void CreateControllers()
    {
       // m_carPhysicsInfoController = new CarPhysicsInfoController(m_carPhysicsInfo, m_currentCar);
       // m_controllers.Add(m_carPhysicsInfoController);

       // m_carInputInfoController = new CarInputInfoController(m_carInputInfo, m_inputManager);
       // m_controllers.Add(m_carInputInfoController);

        m_freeCameraController = new FreeCameraController(m_freeCameraModel, m_currentCar.transform);
        m_controllers.Add(m_freeCameraController);
    }
}