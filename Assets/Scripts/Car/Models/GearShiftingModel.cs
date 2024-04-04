using System.Collections;
using System.Collections.Generic;
using Car.Data;
using UnityEngine;

namespace Car.Models
{
    public class GearShiftingModel
    {
        private List<float> m_gearBoxRatios;
        private readonly float m_shiftTime;
        private int m_currentGear;
        private bool m_isShifting;

        public float currentGearRatio => m_gearBoxRatios[m_currentGear];
        
        public int currentGear
        {
            get { return m_currentGear; }
        }

        public GearShiftingModel(CarDesc.GearboxInfo gearboxInfo)
        {
            m_gearBoxRatios = gearboxInfo.gearBoxRatios;
            m_shiftTime = gearboxInfo.shiftTime;
        }
        
        public void ShiftUp(RaceCar car)
        {
            if (!m_isShifting && m_currentGear < m_gearBoxRatios.Count - 1)
            {
                car.StartCoroutine(ChangeGearAsync(m_currentGear++));
            }
        }
    
        public void ShiftDown(RaceCar car)
        {
            if (!m_isShifting && m_currentGear != 0)
            {
                car.StartCoroutine(ChangeGearAsync(m_currentGear--));
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

        private IEnumerator ChangeGearAsync(int nextGear)
        {
            m_isShifting = true;
            m_currentGear = 1;
        
            yield return new WaitForSeconds(m_shiftTime);

            m_currentGear = nextGear;
            m_isShifting = false;
        }
    }
}
