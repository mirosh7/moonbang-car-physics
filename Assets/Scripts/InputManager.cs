using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
	[SerializeField] private InputActionAsset m_inputActionAsset;

	public static InputManager instance;
	private string m_actionMapName = "GamePlay";

	private float m_acceleration = 0f;
	public float acceleration => m_acceleration;

	private float m_brakes = 0f;
	public float brakes => m_brakes;

	private float m_steering = 0f;
	public float steering => m_steering;

	private Vector2 m_rotation = Vector2.zero;
	public Vector2 rotation => m_rotation;

	public Action gearUp;
	public Action gearDown;

	private InputAction m_steeringAction;
	private InputAction m_brakesAction;
	private InputAction m_gearUpAction;
	private InputAction m_gearDownAction;
	private InputAction m_handBrakeAction;
	private InputAction m_accelerationAction;
	private InputAction m_rotationAction;
	
	private InputAction m_enableEditAction;

	private string m_steeringActionName = "Steering";
	private string m_brakesActionName = "Brakes";
	private string m_gearUpActionName = "Gear Up";
	private string m_gearDownActionName = "Gear Down";
	private string m_handBrakeActionName = "Hand Brake";
	private string m_accelerationActionName = "Acceleration";
	private string m_rotationActionName = "Rotation";

	
	private void Awake()
	{
		if (instance == null)
		{
			instance = this;
			DontDestroyOnLoad(gameObject);
		}
		else
		{
			Destroy(gameObject);
		}

		var currentActionMap = m_inputActionAsset.FindActionMap(m_actionMapName);

		if (currentActionMap != null)
		{
			m_steeringAction = currentActionMap.FindAction(m_steeringActionName);
			m_brakesAction = currentActionMap.FindAction(m_brakesActionName);
			m_gearUpAction = currentActionMap.FindAction(m_gearUpActionName);
			m_gearDownAction = currentActionMap.FindAction(m_gearDownActionName);
			m_handBrakeAction = currentActionMap.FindAction(m_handBrakeActionName);
			m_accelerationAction = currentActionMap.FindAction(m_accelerationActionName);
			m_rotationAction = currentActionMap.FindAction(m_rotationActionName);
		}

		SetRaceActionMapActive(true);
		var persistant = m_inputActionAsset.FindActionMap("General");
		persistant.Enable();
		m_enableEditAction = persistant.FindAction("Edit");
		RegisterInputActions();
	}
	
	private bool m_raceActionMapActive = true;

	private void SetRaceActionMapActive(bool active)
	{
		var currentActionMap = m_inputActionAsset.FindActionMap(m_actionMapName);

		if (active)
		{
			currentActionMap.Enable();
		}
		else
		{
			currentActionMap.Disable();
		}
	}

	private void RegisterInputActions()
	{
		m_steeringAction.performed += ctx => m_steering = ctx.ReadValue<float>();
		m_steeringAction.canceled += ctx => m_steering = 0f;

		m_brakesAction.performed += ctx => m_brakes = ctx.ReadValue<float>();
		m_brakesAction.canceled += ctx => m_brakes = 0f;

		m_rotationAction.performed += ctx => m_rotation = ctx.ReadValue<Vector2>();
		m_rotationAction.canceled += ctx => m_rotation = Vector2.zero;

		m_gearUpAction.performed += ctx => gearUp.Invoke();
		m_gearDownAction.performed += ctx => gearDown.Invoke();

		m_accelerationAction.performed += ctx => m_acceleration = ctx.ReadValue<float>();
		m_accelerationAction.canceled += ctx => m_acceleration = 0f;
		
		m_enableEditAction.performed += ctx => SetRaceActionMapActive(!m_raceActionMapActive);
	}

	private void OnEnable()
	{
		//m_steeringAction.Enable();
		//m_brakesAction.Enable();
		//m_gearUpAction.Enable();
		//m_gearDownAction.Enable();
		//m_accelerationAction.Enable();
		//m_rotationAction.Enable();
		//m_enableEditAction.Enable();
	}

	private void OnDisable()
	{
		//m_steeringAction.Disable();
		//m_brakesAction.Disable();
		//m_gearUpAction.Disable();
		//m_gearDownAction.Disable();
		//m_accelerationAction.Disable();
		//m_rotationAction.Disable();
	}
}