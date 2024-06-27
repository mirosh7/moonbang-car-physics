using System.Collections.Generic;
using System.Linq;
using Car;
using Car.Controllers;
using Car.Controllers.PhysicsControllers;
using Car.Controllers.PhysicsControllers.WheelControllers;
using Car.Data;
using Car.Models;
using Car.Models.PhysicsModels;
using Car.Models.PhysicsModels.WheelModels;
using UnityEngine;

public class CarBuilder
{
    private List<ICarController> m_carControllers = new List<ICarController>();
    private CarDesc m_carDesc;
    private Rigidbody m_rb;
    private List<Transform> m_wheelTransforms = new List<Transform>();
    private List<Transform> m_wheelRootTransforms = new List<Transform>();
    
    private SteeringModel m_steeringModel;
    private GearShiftingModel m_gearShiftingModel;
    private EngineModel m_engineModel;
    private DifferentialModel m_differentialModel;
    private ClutchModel m_clutchModel;
    private BrakesModel m_brakesModel;
    private AntirollBarModel m_antirollBarModel;
    private AccelerationWheelSystemModel m_accelerationWheelSystemModel;
    private RaycastWheelSystemModel m_raycastWheelSystemModel;
    private SlipForcesSystemModel m_slipForcesSystemModel;
    private SuspensionForcesSystemModel m_suspensionForcesSystemModel;
    private TireForceSystemModel m_tireForceSystemModel;
    private VisualWheelSystemModel m_visualWheelSystemModel;
    private CarPrefabLoadModel m_carPrefabLoadModel;
    private WheelSteeringSystemModel m_wheelSteeringSystemModel;
    private WheelSoundSystemModel m_wheelSoundSystemModel;

    private SteeringController m_steeringController;
    private GearboxSystemController m_gearboxSystemController;
    private EngineController m_engineController;
    private DifferentialController m_differentialController;
    private BrakesController m_brakesController;
    private AccelerationWheelSystemController m_accelerationWheelSystemController;
    private RaycastWheelSystemController m_raycastWheelSystemController;
    private SlipForcesSystemController m_slipForcesSystemController;
    private SuspensionForcesSystemController m_suspensionForcesSystemController;
    private TireForcesSystemController m_tireForcesSystemController;
    private VisualWheelSystemController m_visualWheelSystemController;
    private CarPrefabLoadController m_carPrefabLoadController;
    private WheelSteeringSystemController m_wheelSteeringSystemController;
    private WheelSoundSystemController m_wheelSoundSystemController;
    
    private string m_carPrefabName;
    private string m_carWheelName;

    private InputManager m_inputManager;
    
    public SteeringModel steeringModel => m_steeringModel;
    public GearShiftingModel gearShiftingModel => m_gearShiftingModel;
    public EngineModel engineModel => m_engineModel;
    public DifferentialModel differentialModel => m_differentialModel;
    public ClutchModel clutchModel => m_clutchModel;
    public BrakesModel brakesModel => m_brakesModel;
    public AntirollBarModel antirollBarModel => m_antirollBarModel;
    public AccelerationWheelSystemModel accelerationWheelSystemModel => m_accelerationWheelSystemModel;
    public RaycastWheelSystemModel raycastWheelSystemModel => m_raycastWheelSystemModel;
    public SlipForcesSystemModel slipForcesSystemModel => m_slipForcesSystemModel;
    public SuspensionForcesSystemModel suspensionForcesSystemModel => m_suspensionForcesSystemModel;
    public TireForceSystemModel tireForceSystemModel => m_tireForceSystemModel;
    public VisualWheelSystemModel visualWheelSystemModel => m_visualWheelSystemModel;
    public CarPrefabLoadModel carPrefabLoadModel => m_carPrefabLoadModel;

    public CarBuilder(CarDesc carDesc, string carPrefabName, string carWheelName)
    {
        m_inputManager = InputManager.instance;
        m_carDesc = carDesc;
        m_carPrefabName = carPrefabName;
        m_carWheelName = carWheelName;
    }
    
    private void CreateCarModels()
    {
        m_steeringModel = new SteeringModel(m_carDesc.steeringInfo, m_wheelRootTransforms);
        m_gearShiftingModel = new GearShiftingModel(m_carDesc.gearboxInfo);
        m_engineModel = new EngineModel(m_rb, m_carDesc.engineInfo);
        m_differentialModel = new DifferentialModel(m_carDesc.differentialInfo, m_carDesc.wheelInfos[0]);
        m_clutchModel = new ClutchModel(m_carDesc.clutchInfo, m_carDesc.engineInfo);
        m_brakesModel = new BrakesModel(m_carDesc.brakesInfo);
        m_antirollBarModel = new AntirollBarModel(m_carDesc.antirollBarInfo);
        m_accelerationWheelSystemModel = new AccelerationWheelSystemModel(m_carDesc.wheelInfos);
        m_raycastWheelSystemModel = new RaycastWheelSystemModel(m_carDesc.wheelInfos);
        m_slipForcesSystemModel = new SlipForcesSystemModel(m_carDesc.wheelInfos);
        m_suspensionForcesSystemModel = new SuspensionForcesSystemModel(m_carDesc.wheelInfos, m_rb);
        m_tireForceSystemModel = new TireForceSystemModel(m_carDesc.wheelInfos, m_rb);
        m_visualWheelSystemModel = new VisualWheelSystemModel(m_carDesc.wheelInfos);
        m_wheelSteeringSystemModel = new WheelSteeringSystemModel(m_wheelRootTransforms);
        m_wheelSoundSystemModel = new WheelSoundSystemModel(m_wheelRootTransforms);
        
        Debug.Log("Car models created");
    }

    private void CreateCarControllers()
    {
        m_steeringController = new SteeringController(m_steeringModel, m_inputManager, m_slipForcesSystemModel);
        AddToControllersList(m_steeringController);
        
        m_gearboxSystemController = new GearboxSystemController(m_gearShiftingModel, m_clutchModel, m_differentialModel, m_engineModel, m_inputManager);
        AddToControllersList(m_gearboxSystemController);

        m_engineController = new EngineController(m_engineModel, m_clutchModel, m_gearShiftingModel, m_inputManager);
        AddToControllersList(m_engineController);

        m_differentialController = new DifferentialController(m_differentialModel, m_gearShiftingModel, m_clutchModel, m_accelerationWheelSystemModel);
        AddToControllersList(m_differentialController);

        m_brakesController = new BrakesController(m_brakesModel, m_accelerationWheelSystemModel, m_inputManager);
        AddToControllersList(m_brakesController);
        
        m_wheelSteeringSystemController = new WheelSteeringSystemController(m_wheelSteeringSystemModel, m_steeringModel);
        AddToControllersList(m_wheelSteeringSystemController);
        
        m_raycastWheelSystemController = new RaycastWheelSystemController(m_wheelRootTransforms, m_raycastWheelSystemModel);
        AddToControllersList(m_raycastWheelSystemController);
        
        m_visualWheelSystemController = new VisualWheelSystemController(m_visualWheelSystemModel, m_accelerationWheelSystemModel, m_suspensionForcesSystemModel, m_steeringModel, m_wheelTransforms, m_wheelRootTransforms);
        AddToControllersList(m_visualWheelSystemController);
        
        m_suspensionForcesSystemController = new SuspensionForcesSystemController(m_suspensionForcesSystemModel, m_raycastWheelSystemModel, m_wheelRootTransforms);
        AddToControllersList(m_suspensionForcesSystemController);

        m_accelerationWheelSystemController = new AccelerationWheelSystemController(m_accelerationWheelSystemModel, m_brakesModel, m_differentialModel, m_tireForceSystemModel, m_raycastWheelSystemModel);
        AddToControllersList(m_accelerationWheelSystemController);

        m_slipForcesSystemController = new SlipForcesSystemController(m_slipForcesSystemModel, m_suspensionForcesSystemModel, m_accelerationWheelSystemModel, m_raycastWheelSystemModel);
        AddToControllersList(m_slipForcesSystemController);
        
        m_tireForcesSystemController = new TireForcesSystemController(m_tireForceSystemModel, m_wheelRootTransforms, m_raycastWheelSystemModel, m_slipForcesSystemModel, m_suspensionForcesSystemModel);
        AddToControllersList(m_tireForcesSystemController);

        m_wheelSoundSystemController = new WheelSoundSystemController(m_wheelSoundSystemModel, m_tireForceSystemModel);
        AddToControllersList(m_wheelSoundSystemController);
        
        Debug.Log("Car controllers created");
    }

    private void CreateVisualCar()
    {
        m_carPrefabLoadModel = new CarPrefabLoadModel(m_carPrefabName, m_carWheelName);
        m_carPrefabLoadController = new CarPrefabLoadController(m_carPrefabLoadModel);
    }

    public RaceCar BuildCar(Transform spawnPoint)
    {
        CreateVisualCar();
        var carCore = Object.Instantiate(m_carPrefabLoadController.carCore, spawnPoint.position, spawnPoint.rotation);
        var carVisual = Object.Instantiate(m_carPrefabLoadController.carPrefab, carCore.transform);
        
        foreach (var w in m_carDesc.wheelInfos)
        {
            var wheel = Object.Instantiate(m_carPrefabLoadController.wheelPrefab, carCore.transform);
            m_wheelTransforms.Add(wheel.transform);
        }
        
        var raceCar = carCore.GetComponent<RaceCar>();
        m_rb = carCore.GetComponent<Rigidbody>();
        
        m_wheelRootTransforms = carVisual.GetComponentsInChildren<Transform>().Where(wT => wT.name == "wheelRoot").ToList();
        CreateCarModels();
        CreateCarControllers();
        
        raceCar.SetControllers(m_carControllers);
        var engineSoundController = carCore.GetComponentInChildren<RealisticEngineSound>();
        engineSoundController.engineModel = m_engineModel;
        
        return raceCar;
    }
    
    private void AddToControllersList(ICarController carController)
    {
        m_carControllers.Add(carController);
    }
}