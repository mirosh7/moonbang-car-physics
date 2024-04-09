using System;
using System.Collections.Generic;
using Car.Data;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Car.Models.PhysicsModels
{
    public class GearShiftingModel
    {
        private List<float> m_gearBoxRatios;
        private readonly float m_shiftTime;
        private int m_currentGear;
        private bool m_isShifting;

        public float currentGearRatio => m_gearBoxRatios[m_currentGear];
        
        public int currentGear => m_currentGear;

        public GearShiftingModel(CarDesc.GearboxInfo gearboxInfo)
        {
            m_gearBoxRatios = gearboxInfo.gearBoxRatios;
            m_shiftTime = gearboxInfo.shiftTime;
        }
        
        public async void ShiftUp()
        {
            if (!m_isShifting && m_currentGear < m_gearBoxRatios.Count - 1)
            {
                await ChangeGearAsync(++m_currentGear);
                Debug.Log($"GEAR UP {m_currentGear}");
            }
            
        }
    
        public async void ShiftDown()
        {
            if (!m_isShifting && m_currentGear != 0)
            {
                await ChangeGearAsync(--m_currentGear);
                Debug.Log($"GEAR DOWN {m_currentGear}");
            }
        }
    
        public float GetInputShaftVelocity(float diffShaftVelocity)
        {
            float inputShaftVelocity = diffShaftVelocity * currentGearRatio;
            return inputShaftVelocity;
        }
        
        public float GetOutputTorque(float inputTorque)
        {
            float outputTorque = inputTorque * m_gearBoxRatios[m_currentGear];
            return outputTorque; 
        }
        
        private async UniTask ChangeGearAsync(int nextGear)
        {
            m_isShifting = true;
            m_currentGear = 1;
            
            await UniTask.Delay(TimeSpan.FromSeconds(m_shiftTime));

            m_currentGear = nextGear;
            m_isShifting = false;
        }
    }
}
