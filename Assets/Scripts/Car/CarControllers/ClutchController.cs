using Car.Core;
using UnityEngine;

namespace Car.CarControllers
{
    public class ClutchController : ICarController
    {
        private EngineController m_engine;
        private GearboxController m_gearBox;
        
        private float m_clutchStiffness;
        private float m_clutchCapacity;
        private float m_engineMaxTorque;
        private float m_clutchDamping;
        private float m_clutchTorque;
        private float m_clutchMaxTorque;
        private float m_outputShaftVelocity;
        private float m_engineAngularVelocity;
        private float m_gearBoxRatio;
        private float m_radToRpm;
        private float m_rpmToRad;
        private float m_clutchLock;

        public ClutchController(EngineController engine, GearboxController gearBox)
        {
            m_engine = engine;
            m_gearBox = gearBox;
            m_clutchMaxTorque = engine.engineMaxTorque * m_clutchCapacity;
        }

        public void OnUpdatePhysics()
        {
            throw new System.NotImplementedException();
        }
    
        public float GetClutchTorque()
        {
            var engineAngularVelocity = m_engine.engineAngularVelocity;
            var currentGearBoxRatio = m_gearBox.currentGearRatio;
        
            var clutchSlip = (engineAngularVelocity - m_gearBox.GetInputShaftVelocity()) * Mathf.Abs(Mathf.Sign(currentGearBoxRatio));
            m_clutchLock = currentGearBoxRatio == 0 ? 0 : MapRangeClamped(engineAngularVelocity * EngineController.RAD_TO_RPM, 1000, 1300, 0, 1);
            var clt = Mathf.Clamp(clutchSlip * m_clutchLock * m_clutchStiffness, -m_clutchMaxTorque, m_clutchMaxTorque);
        
            return m_clutchTorque = clt + ((m_clutchTorque - clt) * m_clutchDamping);
        }


        private float MapRangeClamped(float value, float inRangeA, float inRangeB, float outRangeA, float outRangeB)
        {
            float result = Mathf.Lerp(outRangeA, outRangeB, Mathf.InverseLerp(inRangeA, inRangeB, value));
            return (result);
        }
    }
}
