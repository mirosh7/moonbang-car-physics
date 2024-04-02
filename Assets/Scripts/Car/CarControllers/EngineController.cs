using Car.Core;
using UnityEngine;

namespace Car.CarControllers
{
    public class EngineController : ICarController
    {
        public const float RPM_TO_RAD = Mathf.PI * 2f / 60f;
        public const float RAD_TO_RPM = 1/RPM_TO_RAD;
    
        private Rigidbody m_carRigidbody;
        private GearboxController m_gearBoxController;
        private ClutchController m_clutchController;
        private InputManager m_input;
        private AnimationCurve m_torqueCurve;

        private Vector3 m_engineOrientation;
    
        private float m_engineIdleRpm;
        private float m_engineRpm;
        private float m_engineMaxRpm;
    
        private float m_maxEffectiveTorque;
        private float m_engineTorque;
        private float m_engineMul;
        private float m_engineFriction;
        private float m_engineFrictionCoefficient;
        private float m_engineAcceleration;
        private float m_engineAngularVelocity;
        private float m_startFriction;
        private float m_engineInertia;

        public float engineAngularVelocity => m_engineAngularVelocity;
        public float engineMaxTorque => GetMaxValueFromAnimationCurve(m_torqueCurve);
    
        public EngineController(Rigidbody carRigidbody, GearboxController gearBoxController, ClutchController clutchController, InputManager input)
        {
            m_carRigidbody = carRigidbody;
            m_gearBoxController = gearBoxController;
            m_clutchController = clutchController;
            m_input = input;
        }

        public void OnUpdatePhysics()
        {
            UpdateEngine();
        }

        private void UpdateEngine()
        {
            m_maxEffectiveTorque = m_torqueCurve.Evaluate(m_engineRpm) * m_engineMul;
            m_engineFriction = (m_engineRpm * m_engineFrictionCoefficient) + m_startFriction;
            m_engineTorque = m_maxEffectiveTorque * m_input.throttle - m_engineFriction ;
            m_engineAcceleration = (m_engineTorque - m_clutchController.GetClutchTorque()) / m_engineInertia ;
            m_engineAngularVelocity += m_engineAcceleration * Time.fixedDeltaTime;
            m_engineRpm = m_engineAngularVelocity * RAD_TO_RPM;
            m_engineAngularVelocity = Mathf.Clamp(m_engineAngularVelocity, m_engineIdleRpm, m_engineMaxRpm);
        
            if (m_gearBoxController.currentGearRatio == 0)
            {
                m_carRigidbody.AddTorque(m_engineOrientation * m_engineTorque * 2f);
            }
        }
    
        private float GetMaxValueFromAnimationCurve(AnimationCurve curve)
        {
            float maxValue = float.MinValue;

            for (int i = 0; i < curve.length; i++)
            {
                float keyValue = curve.keys[i].value;

                if (keyValue > maxValue)
                {
                    maxValue = keyValue;
                }
            }

            return maxValue;
        }
    }
}
