using Car.Data;
using UnityEngine;

namespace Car.Models.PhysicsModels.WheelComponents
{
    public class TireForceComponent
    {
        private CarDesc.WheelInfo m_wheelInfo;
        private Rigidbody m_rb;
        private float m_fx;
        private Vector2 m_combinedForce;

        public float fx => m_fx;
        public Vector2 normalizedForce => m_combinedForce.normalized;

        public TireForceComponent(CarDesc.WheelInfo wheelInfo, Rigidbody rb)
        {
            m_rb = rb;
            m_wheelInfo = wheelInfo;
        }

        public void UpdateTireForce(Transform wheelRoot, RaycastHit hit, float longitudinalForce, float lateralForce,
            float suspensionForce)
        {
            Vector3 forwardForceVectorNormalized = Vector3.ProjectOnPlane(wheelRoot.forward, hit.normal).normalized;
            Vector3 sideForceVectorNormalized = Vector3.ProjectOnPlane(wheelRoot.right, hit.normal).normalized;
            m_combinedForce = new Vector2(longitudinalForce, lateralForce);

            if (m_combinedForce.magnitude > 1)
            {
                m_combinedForce = m_combinedForce.normalized;
            }

            m_fx = m_combinedForce.x * suspensionForce * m_wheelInfo.longitudinalCoeff;

            var fY = m_combinedForce.y * suspensionForce *m_wheelInfo.lateralCoeff;

            Vector3 combinedForceNorm = (forwardForceVectorNormalized * m_fx + sideForceVectorNormalized * fY);
            Debug.DrawRay(hit.point, combinedForceNorm, Color.yellow);
            m_rb.AddForceAtPosition(combinedForceNorm, hit.point);
        }
    }
}
