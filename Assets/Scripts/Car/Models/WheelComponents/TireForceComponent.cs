using Car.Data;
using UnityEngine;

namespace Car.Models.WheelComponents
{
    public class TireForceComponent
    {
        private float m_longitudinalCoeff;
        private float m_lateralCoeff;
        private float m_fx;

        public float fx => m_fx;

        public TireForceComponent(CarDesc.WheelInfo wheelInfo)
        {
            m_longitudinalCoeff = wheelInfo.longitudinalCoeff;
            m_lateralCoeff = wheelInfo.lateralCoeff;
        }

        public void UpdateTireForce(Rigidbody rb, Transform wheelRoot, RaycastHit hit, float longitudinalForce, float lateralForce, float suspensionForce)
        {
            Vector3 forwardForceVectorNormalized = Vector3.ProjectOnPlane(wheelRoot.forward, hit.normal).normalized;
            Vector3 sideForceVectorNormalized = Vector3.ProjectOnPlane(wheelRoot.right, hit.normal).normalized;
            Vector2 combinedForce = new Vector2(longitudinalForce, lateralForce);

            if (combinedForce.magnitude > 1)
            {
                combinedForce = combinedForce.normalized;
            }

            m_fx = combinedForce.x * suspensionForce * m_longitudinalCoeff;
            var fY = combinedForce.y * suspensionForce * m_lateralCoeff;
                
            Vector3 combinedForceNorm = (forwardForceVectorNormalized * m_fx + sideForceVectorNormalized * fY);
            Debug.DrawRay(hit.point, combinedForceNorm, Color.red);
            rb.AddForceAtPosition(combinedForceNorm, hit.point);
        }
    }
}
