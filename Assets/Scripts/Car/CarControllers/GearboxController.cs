using System.Collections;
using System.Collections.Generic;
using Car.Core;
using UnityEngine;

namespace Car.CarControllers
{
    public class GearboxController : ICarController
    {
        private RaceCar m_raceCar;
        private DifferentialController m_differential;
        private List<float> m_gearBoxRatios = new List<float>();
        private float m_shiftTime;
        private int m_currentGear;
        private bool m_isShifting;

        public float currentGearRatio => m_gearBoxRatios[m_currentGear];

        public GearboxController(RaceCar car, DifferentialController differentialController)
        {
            m_raceCar = car;
            m_differential = differentialController;
        }

        public void OnUpdatePhysics()
        {
            throw new System.NotImplementedException();
        }

        public void ShiftUp()
        {
            if (!m_isShifting && m_currentGear < m_gearBoxRatios.Count - 1)
            {
                m_raceCar.StartCoroutine(ChangeGearAsync(m_currentGear++));
            }
        }
    
        public void ShiftDown()
        {
            if (!m_isShifting && m_currentGear != 0)
            {
                m_raceCar.StartCoroutine(ChangeGearAsync(m_currentGear--));
            }
        }
    
        public float GetInputShaftVelocity()
        {
            float inputShaftVelocity = m_differential.GetInputShaftVelocity() * currentGearRatio;
            return inputShaftVelocity;
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
