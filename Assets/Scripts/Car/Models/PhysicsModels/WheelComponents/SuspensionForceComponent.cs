using Car.Data;
using UnityEngine;

namespace Car.Models.PhysicsModels.WheelComponents
{
    public class SuspensionForceComponent
    {
        private Rigidbody m_rb;
        private CarDesc.WheelInfo m_wheelInfo;
        private Vector3 m_linearVelocity;
        private float m_suspensionForce;
        private float m_currentLength;
        private float m_lastLength;

        public SuspensionForceComponent(CarDesc.WheelInfo wheelInfo, Rigidbody rb)
        {
            m_rb = rb;
            m_wheelInfo = wheelInfo;
        }

        public Vector3 linearVelocity => m_linearVelocity;

        public float suspensionForce => m_suspensionForce;

        public float currentLength => m_currentLength;

        public void UpdateSuspensionForce(RaycastHit hit, Transform wheelRoot)
        {
            var up = wheelRoot.up;
            m_currentLength = (wheelRoot.position - (hit.point + (up * m_wheelInfo.wheelRadius))).magnitude;
            CalculateSuspensionForce();
            m_rb.AddForceAtPosition(m_suspensionForce * up, hit.point);
            Debug.DrawRay(hit.point, m_suspensionForce * up, Color.blue);
            m_linearVelocity = wheelRoot.InverseTransformDirection(m_rb.GetPointVelocity(hit.point));
        }
        
        private void CalculateSuspensionForce()
        {
            var springForce = (m_wheelInfo.restLength - m_currentLength) * m_wheelInfo.suspensionStiffness;
            var damperForce = (m_lastLength - m_currentLength) / Time.fixedDeltaTime * m_wheelInfo.damperStiffness;
            m_suspensionForce = Mathf.Max(0f, springForce + damperForce);
            m_lastLength = m_currentLength;
        }
    }
}
