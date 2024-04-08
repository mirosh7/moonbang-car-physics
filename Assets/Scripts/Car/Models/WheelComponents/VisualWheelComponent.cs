using Car.Data;
using UnityEngine;

namespace Car.Models.WheelComponents
{
    public class VisualWheelComponent
    {
        private float m_camber;
        private float m_caster;
        
        public VisualWheelComponent(CarDesc.WheelInfo wheelInfo)
        {
            m_camber = wheelInfo.camber;
            m_caster = wheelInfo.caster;
        }

        public void ApplyVisuals(Transform wheelVisual, Transform wheelRoot, float angularVelocity, float currentLength, float steerAngle)
        {
            var currentAngle = angularVelocity * Mathf.Rad2Deg * Time.deltaTime;
            currentAngle %= 360f;

            wheelVisual.position = wheelRoot.position - wheelRoot.up * currentLength;
            wheelVisual.localRotation = Quaternion.Euler(currentAngle, steerAngle, 0f);
            //wheelVisual.parent.transform.localRotation = Quaternion.Euler(m_caster, 0, m_camber);
            wheelVisual.transform.localRotation = Quaternion.Euler(m_caster, 0, m_camber);
        }
    }
}
