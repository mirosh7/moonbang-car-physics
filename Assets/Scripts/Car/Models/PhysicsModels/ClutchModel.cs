using Car.Data;
using UnityEngine;

namespace Car.Models.PhysicsModels
{
    public class ClutchModel
    {
        private float m_clutchStiffness;
        private float m_clutchCapacity;
        private float m_clutchDamping;
        private float m_clutchTorque;
        private float m_clutchMaxTorque;
        private float m_clutchLock;
        
        public float clutchTorque => m_clutchTorque;
        public float clutchLock => m_clutchLock;

        

        public ClutchModel(CarDesc.ClutchInfo clutchInfo, CarDesc.EngineInfo engineInfo)
        {
            m_clutchStiffness = clutchInfo.clutchStiffness;
            m_clutchCapacity = clutchInfo.clutchCapacity;
            m_clutchDamping = clutchInfo.clutchDamping;
            m_clutchMaxTorque = engineInfo.maxEngineTorque * m_clutchCapacity;
        }
        
        public void UpdateClutchTorque(float engineAngularVelocity, float currentGearBoxRatio, float gearBoxInputShaftVelocity)
        {
            var clutchSlip = (engineAngularVelocity - gearBoxInputShaftVelocity) * Mathf.Abs(Mathf.Sign(currentGearBoxRatio));
            m_clutchLock = currentGearBoxRatio == 0 ? 0 : MapRangeClamped(engineAngularVelocity * EngineModel.RAD_TO_RPM, 1000, 1300, 0, 1);
            var clt = Mathf.Clamp(clutchSlip * m_clutchLock * m_clutchStiffness, -m_clutchMaxTorque, m_clutchMaxTorque);
            m_clutchTorque = clt + ((m_clutchTorque - clt) * m_clutchDamping);
        }


        private float MapRangeClamped(float value, float inRangeA, float inRangeB, float outRangeA, float outRangeB)
        {
            float result = Mathf.Lerp(outRangeA, outRangeB, Mathf.InverseLerp(inRangeA, inRangeB, value));
            return (result);
        }
    }
}
