using Car.Core;
using UnityEngine;

namespace Car.CarControllers
{
    public class WheelController : ICarController
    {

        private Transform m_wheelTransform;
        
        private float m_angularVelocity;

        private float m_wheelInertia;

        public float angularVelocity => m_angularVelocity;
        public float wheelInertia => m_wheelInertia;

        public Transform transform => m_wheelTransform;

        public WheelController(Transform wheelTransform)
        {
            m_wheelTransform = wheelTransform;
        }
        
        public void OnUpdatePhysics()
        {
            throw new System.NotImplementedException();
        }
    }
}
