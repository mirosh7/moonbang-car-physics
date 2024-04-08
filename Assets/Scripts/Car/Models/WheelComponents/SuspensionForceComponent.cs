using Car.Data;
using UnityEngine;

namespace Car.Models.WheelComponents
{
    public class SuspensionForceComponent
    {
        private Rigidbody m_rb;
        private float m_restLength;
        private float m_suspensionStiffness;
        private float m_damperStiffness;
        private Vector3 m_linearVelocity;
        private float m_suspensionForce;
        private float m_currentLength;
        private float m_lastLength;
        private float m_wheelRadius;

        public SuspensionForceComponent(CarDesc.WheelInfo wheelInfo, Rigidbody rb)
        {
            m_rb = rb;
            m_restLength = wheelInfo.restLength;
            m_suspensionStiffness = wheelInfo.suspensionStiffness;
            m_damperStiffness = wheelInfo.damperStiffness;
            m_wheelRadius = wheelInfo.wheelRadius;
        }

        public Vector3 linearVelocity => m_linearVelocity;

        public float suspensionForce => m_suspensionForce;

        public float currentLength => m_currentLength;

        public void UpdateSuspensionForce(RaycastHit hit, Transform wheelRoot)
        {
            var up = wheelRoot.up;
            m_currentLength = (wheelRoot.position - (hit.point + (up * m_wheelRadius))).magnitude;
            CalculateSuspensionForce();
            m_rb.AddForceAtPosition(m_suspensionForce * up, hit.point);
            m_linearVelocity = wheelRoot.InverseTransformDirection(m_rb.GetPointVelocity(hit.point));
        }
        
        private void CalculateSuspensionForce()
        {
            var springForce = (m_restLength - m_currentLength) * m_suspensionStiffness;
            var damperForce = (m_lastLength - m_currentLength) / Time.fixedDeltaTime * m_damperStiffness;
            m_suspensionForce = Mathf.Max(0, springForce + damperForce);
            m_lastLength = m_currentLength;
        }
    }
}
