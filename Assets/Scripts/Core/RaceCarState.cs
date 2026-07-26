using System.Collections.Generic;
using Camera.Controllers;
using Camera.Models;
using Car;
using Car.Data;
using Cinemachine;
using UnityEngine;

public class RaceCarState : MonoBehaviour
{
    [SerializeField]
    private CarDesc m_carDesc;
    [SerializeField]
    private Transform m_carSpawnPoint;
    [SerializeField]
    private CinemachineFreeLook m_carCamera;

    private readonly List<IController> m_controllers = new List<IController>();

    private InputManager m_inputManager;
    private RaceCar m_currentCar;
    private CarBuilder m_carBuilder;

    private FreeCameraModel m_freeCameraModel;
    private FreeCameraController m_freeCameraController;

    public CarBuilder carBuilder => m_carBuilder;

    private void Start()
    {
        m_inputManager = InputManager.instance;
        m_carBuilder = new CarBuilder(m_carDesc, "porsche", "porscheWheel");
        m_currentCar = m_carBuilder.BuildCar(m_carSpawnPoint);
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

    private void CreateModels()
    {
        m_freeCameraModel = new FreeCameraModel(m_carCamera, m_inputManager);
    }

    private void CreateControllers()
    {
        m_freeCameraController = new FreeCameraController(m_freeCameraModel, m_currentCar.transform);
        m_controllers.Add(m_freeCameraController);
    }
}
