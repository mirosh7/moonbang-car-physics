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
	public float clutch;
	public float handbrake;

	public bool blockCar = false;

	private InputAction m_steeringAction;
	private InputAction m_brakesAction;
	private InputAction m_gearUpAction;
	private InputAction m_gearDownAction;
	private InputAction m_handBrakeAction;
	private InputAction m_accelerationAction;
	private InputAction m_rotationAction;
	private InputAction m_clutchAction;

	private InputAction m_enableEditAction;

	private string m_steeringActionName = "Steering";
	private string m_brakesActionName = "Brakes";
	private string m_gearUpActionName = "Gear Up";
	private string m_gearDownActionName = "Gear Down";
	private string m_handBrakeActionName = "HandBrake";
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

		blockCar = false;
		m_raceActionMapActive = true;
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
			m_clutchAction = currentActionMap.FindAction("Clutch");
			currentActionMap.Enable();
		}

		var persistant = m_inputActionAsset.FindActionMap("General");
		persistant.Enable();
		m_enableEditAction = persistant.FindAction("Edit");
		RegisterInputActions();
	}

	private void OnDestroy()
	{
		m_enableEditAction.performed -= SwitchActionMapActive;
	}

	private bool m_raceActionMapActive = true;

	private void SwitchActionMapActive(InputAction.CallbackContext ctx)
	{
		m_raceActionMapActive = !m_raceActionMapActive;

		var currentActionMap = m_inputActionAsset.FindActionMap(m_actionMapName);

		if (m_raceActionMapActive)
		{
			blockCar = false;
			currentActionMap.Enable();
		}
		else
		{
			blockCar = true;
			currentActionMap.Disable();
		}
	}

	private void RegisterInputActions()
	{
		m_steeringAction.performed += ctx => m_steering = ctx.ReadValue<float>();
		m_steeringAction.canceled += ctx => m_steering = 0f;

		m_brakesAction.performed += ctx => m_brakes = ctx.ReadValue<float>();
		m_brakesAction.canceled += ctx => m_brakes = 0f;
		
		m_clutchAction.performed += ctx => clutch = ctx.ReadValue<float>();
		m_clutchAction.canceled += ctx => clutch = 0f;

		m_rotationAction.performed += ctx => m_rotation = ctx.ReadValue<Vector2>();
		m_rotationAction.canceled += ctx => m_rotation = Vector2.zero;

		m_gearUpAction.performed += ctx => gearUp.Invoke();
		m_gearDownAction.performed += ctx => gearDown.Invoke();

		m_accelerationAction.performed += ctx => m_acceleration = ctx.ReadValue<float>();
		m_accelerationAction.canceled += ctx => m_acceleration = 0f;
		
		m_handBrakeAction.performed += ctx => handbrake = ctx.ReadValue<float>();
		m_handBrakeAction.canceled += ctx => handbrake = 0f;

		m_enableEditAction.performed += SwitchActionMapActive;
	}
}