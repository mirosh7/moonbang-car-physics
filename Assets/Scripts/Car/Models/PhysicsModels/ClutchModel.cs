using Car.Data;
using UnityEngine;

namespace Car.Models.PhysicsModels
{
    public class ClutchModel
    {
        private CarDesc.ClutchInfo m_clutchInfo;
        private CarDesc.EngineInfo m_engineInfo;
        private float m_clutchTorque;
        private float m_clutchLock;
        
        public float clutchTorque => m_clutchTorque;
        public float clutchLock => m_clutchLock;
        
        public ClutchModel(CarDesc.ClutchInfo clutchInfo, CarDesc.EngineInfo engineInfo)
        {
            m_clutchInfo = clutchInfo;
            m_engineInfo = engineInfo;
        }
        
        public void UpdateClutchTorque(float engineAngularVelocity, float currentGearBoxRatio, float gearBoxInputShaftVelocity)
        {
            var clutchMaxTorque = m_engineInfo.maxEngineTorque * m_clutchInfo.clutchCapacity;
            var clutchSlip = (engineAngularVelocity - gearBoxInputShaftVelocity) * Mathf.Abs(Mathf.Sign(currentGearBoxRatio));
            m_clutchLock = currentGearBoxRatio == 0 ? 0 : MapRangeClamped(engineAngularVelocity * EngineModel.RAD_TO_RPM, 1000, 1300, 0, 1);
            var clt = Mathf.Clamp(clutchSlip * m_clutchLock * m_clutchInfo.clutchStiffness, -clutchMaxTorque, clutchMaxTorque);
            m_clutchTorque = clt + ((m_clutchTorque - clt) * m_clutchInfo.clutchDamping);
        }


        private float MapRangeClamped(float value, float inRangeA, float inRangeB, float outRangeA, float outRangeB)
        {
            float result = Mathf.Lerp(outRangeA, outRangeB, Mathf.InverseLerp(inRangeA, inRangeB, value));
            return (result);
        }
    }
}
