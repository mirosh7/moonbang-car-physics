using System.Collections.Generic;
using Car.Data;
using Car.Models.WheelComponents;
using UnityEngine;

namespace Car.Models.WheelModels
{
    public class SuspensionForcesSystemModel
    {
        private List<SuspensionForceComponent> m_suspensionForceComponents = new List<SuspensionForceComponent>();
        private List<float> m_suspensionForces = new List<float>();
        private List<float> m_currentLengths = new List<float>();
        private List<Vector3> m_linearVelocities = new List<Vector3>();

        public List<Vector3> linearVelocities => m_linearVelocities;
        public List<float> suspensionForces => m_suspensionForces;
        public List<float> currentLengths => m_currentLengths;

        public SuspensionForcesSystemModel(List<CarDesc.WheelInfo> wheelInfos, Rigidbody rb)
        {
            foreach (var wheelInfo in wheelInfos)
            {
                m_suspensionForceComponents.Add(new SuspensionForceComponent(wheelInfo, rb));
            }
            
            foreach (var suspensionForceComponent in m_suspensionForceComponents)
            {
                m_suspensionForces.Add(suspensionForceComponent.suspensionForce);
                m_currentLengths.Add(suspensionForceComponent.currentLength);
                m_linearVelocities.Add(suspensionForceComponent.linearVelocity);
            }
        }

        public void UpdateWheelsSuspension(List<RaycastHit> raycastHits, List<Transform> wheelRoots)
        {
            for (int i = 0; i < m_suspensionForceComponents.Count; i++)
            {
                m_suspensionForceComponents[i].UpdateSuspensionForce(raycastHits[i], wheelRoots[i]);
                m_suspensionForces[i] = m_suspensionForceComponents[i].suspensionForce;
                m_currentLengths[i] = m_suspensionForceComponents[i].currentLength;
                m_linearVelocities[i] = m_suspensionForceComponents[i].linearVelocity;
            }
        }
    }
}
