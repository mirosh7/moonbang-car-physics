using System.Collections.Generic;
using Car.Data;
using Car.Models.PhysicsModels.WheelComponents;
using Unity.Jobs;
using UnityEngine;

namespace Car.Models.PhysicsModels.WheelModels
{
    public class SlipForcesSystemModel
    {
        private List<SlipForceComponent> m_slipForceComponents = new List<SlipForceComponent>();
        private List<Vector2> m_slipForces = new List<Vector2>();
        private List<float> m_slipAngles = new List<float>();
        private List<float> m_lateralAccelerations = new List<float>();

        public List<Vector2> slipForces => m_slipForces;
        public List<float> slipAngles => m_slipAngles;
        public List<float> lateralAccelerations => m_lateralAccelerations;

        public SlipForcesSystemModel(List<CarDesc.WheelInfo> wheelInfos)
        {
            foreach (var wheelInfo in wheelInfos)
            {
                m_slipForceComponents.Add(new SlipForceComponent(wheelInfo));
            }
           
            foreach (var slipForceComponent in m_slipForceComponents)
            {
                m_slipForces.Add(slipForceComponent.slipForce);
                m_slipAngles.Add(slipForceComponent.slipAngle);
                m_lateralAccelerations.Add(slipForceComponent.lateralAcceleration);
            }
        } 

        public void UpdateSlipForces(List<Vector3> linearVelocities, List<float> suspensionForces, List<float> angularVelocities, List<bool> wheelHitStates)
        {
            for (int i = 0; i < m_slipForceComponents.Count; i++)
            {
                if (!wheelHitStates[i])
                {
                    continue;
                }
                
                m_slipForceComponents[i].UpdateSlipForces(linearVelocities[i], suspensionForces[i], angularVelocities[i]);
                m_slipForces[i] = m_slipForceComponents[i].slipForce;
                m_slipAngles[i] = m_slipForceComponents[i].slipAngle;
                m_lateralAccelerations[i] = m_slipForceComponents[i].lateralAcceleration;
            }
        }
    }
}
