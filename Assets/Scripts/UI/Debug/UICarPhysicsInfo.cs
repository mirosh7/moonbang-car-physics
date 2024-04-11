using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace UI.Debug
{
    public class UICarPhysicsInfo : MonoBehaviour
    {
        [SerializeField]
        private List<TMP_Text> m_tmpTexts;
        
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
    }
}
