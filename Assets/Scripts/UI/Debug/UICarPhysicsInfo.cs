using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace UI.Debug
{
    public class UICarPhysicsInfo : MonoBehaviour
    {
        [SerializeField]
        private List<TMP_Text> m_tmpTexts;
        
        [SerializeField]
        private List<TMP_Text> m_slipForcesTexts;
        
        [SerializeField]
        private List<TMP_Text> m_suspensionForcesTexts;
        
        [SerializeField]
        private List<TMP_Text> m_angularVelocitiesForcesTexts;
        
        [SerializeField]
        private List<TMP_Text> m_slipAnglesTexts;
        
        [SerializeField]
        private List<TMP_Text> m_linearVelocitiesTexts;
        
        public void SetEngineRpm(float data)
        {
            m_tmpTexts[0].text = $"Engine RPM = {data}";
        }

        public void SetClutchTorque(float data)
        {
            m_tmpTexts[1].text = $"Clutch Torque = {data}";
        }

        public void SetClutchLock(float data)
        {
            m_tmpTexts[2].text = $"Clutch Lock = {data}";
        }

        public void SetGearValue(float data)
        {
            const string currenGearTitle = "Current Gear";

            switch (data)
            {
                case 0: m_tmpTexts[3].text = $"{currenGearTitle} = R";
                    break;
                case 1: m_tmpTexts[3].text = $"{currenGearTitle} = N";
                    break;
                default:
                    m_tmpTexts[3].text = $"{currenGearTitle} = {data - 1}";
                    break;
            }
        }
        
        public void SetSpeedValue(float data)
        {
            m_tmpTexts[4].text = $"Speed = {data} KM/H";
        }

        public void SetSuspensionForce(IReadOnlyList<float> datas)
        {
            for (int i = 0; i < datas.Count; i++)
            {
                m_suspensionForcesTexts[i].text = $"Wheel {i} SuspensionForce = {datas[i]}";
            }
        }
        
        public void SetAngularVelocities(IReadOnlyList<float> datas)
        {
            for (int i = 0; i < datas.Count; i++)
            {
                m_angularVelocitiesForcesTexts[i].text = $"Wheel {i} AngularVelocity = {datas[i]}";
            }
        }
        
        public void SetSlipForcesValue(IReadOnlyList<Vector2> datas)
        {
            for (int i = 0; i < datas.Count; i++)
            {
                m_slipForcesTexts[i].text = $"Wheel {i} SlipForce = {datas[i]}";
            }
        }
        
        public void SetSlipAngles(IReadOnlyList<float> datas)
        {
            for (int i = 0; i < datas.Count; i++)
            {
                m_slipAnglesTexts[i].text = $"Wheel {i} SlipAngle = {datas[i]}";
            }
        }

        public void SetLinearVelocities(IReadOnlyList<Vector3> datas)
        {
            for (int i = 0; i < datas.Count; i++)
            {
                m_linearVelocitiesTexts[i].text = $"Wheel {i} LinearVelocity = {datas[i]}";
            }
        }
    }
}
