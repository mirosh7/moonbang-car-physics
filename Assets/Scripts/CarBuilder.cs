using System.Collections.Generic;
using Car.Controllers;
using Car.Controllers.WheelControllers;
using Car.Data;
using Car.Models;
using Car.Models.WheelComponents;
using Car.Models.WheelModels;
using UnityEngine;
using UnityEngine.Rendering;

public class CarBuilder
{
    private List<ICarController> m_carControllers = new List<ICarController>();
    private CarDesc m_carDesc;
    private Rigidbody m_rb;
    private List<Transform> m_wheelTransforms;
    private List<Transform> m_wheelRootTransforms;
    
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

    private InputManager m_inputManager;

    public CarBuilder(CarDesc carDesc, List<Transform> wheelTransforms, List<Transform> wheelRootTransforms, Rigidbody rb)
    {
        m_rb = rb;
        m_carDesc = carDesc;
        m_wheelTransforms = wheelTransforms;
        m_wheelRootTransforms = wheelRootTransforms;
    }
    
    public List<ICarController> carControllers => m_carControllers;

    public void Build()
    {
        m_inputManager = new InputManager();
        CreateCarModels();
        CreateCarControllers();
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
        
        Debug.Log("Car models created");
    }

    private void CreateCarControllers()
    {
        m_steeringController = new SteeringController(m_steeringModel, m_inputManager);
        AddToControllersList(m_steeringController);
        
        m_gearboxSystemController = new GearboxSystemController(m_gearShiftingModel, m_clutchModel, m_differentialModel, m_engineModel);
        AddToControllersList(m_gearboxSystemController);

        m_engineController = new EngineController(m_engineModel, m_clutchModel, m_gearShiftingModel, m_inputManager);
        AddToControllersList(m_engineController);

        m_differentialController = new DifferentialController(m_differentialModel, m_gearShiftingModel, m_clutchModel, m_accelerationWheelSystemModel);
        AddToControllersList(m_differentialController);

        m_brakesController = new BrakesController(m_brakesModel, m_accelerationWheelSystemModel, m_inputManager);
        AddToControllersList(m_brakesController);

        m_accelerationWheelSystemController = new AccelerationWheelSystemController(m_accelerationWheelSystemModel, m_brakesModel, m_differentialModel, m_tireForceSystemModel, m_raycastWheelSystemModel);
        AddToControllersList(m_accelerationWheelSystemController);

        m_raycastWheelSystemController = new RaycastWheelSystemController(m_wheelRootTransforms, m_raycastWheelSystemModel);
        AddToControllersList(m_raycastWheelSystemController);

        m_slipForcesSystemController = new SlipForcesSystemController(m_slipForcesSystemModel, m_suspensionForcesSystemModel, m_accelerationWheelSystemModel, m_raycastWheelSystemModel);
        AddToControllersList(m_slipForcesSystemController);

        m_suspensionForcesSystemController = new SuspensionForcesSystemController(m_suspensionForcesSystemModel, m_raycastWheelSystemModel, m_wheelRootTransforms);
        AddToControllersList(m_suspensionForcesSystemController);

        m_tireForcesSystemController = new TireForcesSystemController(m_tireForceSystemModel, m_wheelRootTransforms, m_raycastWheelSystemModel, m_slipForcesSystemModel, m_suspensionForcesSystemModel);
        AddToControllersList(m_tireForcesSystemController);

        m_visualWheelSystemController = new VisualWheelSystemController(m_visualWheelSystemModel, m_accelerationWheelSystemModel, m_suspensionForcesSystemModel, m_steeringModel, m_wheelTransforms, m_wheelRootTransforms);
        AddToControllersList(m_visualWheelSystemController);
        
        Debug.Log("Car controllers created");
    }

    private void AddToControllersList(ICarController carController)
    {
        m_carControllers.Add(carController);
    }
    
}