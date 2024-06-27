using Car.Data;
using UnityEngine;

namespace Car.Models.PhysicsModels.WheelComponents
{
    public class VisualWheelComponent
    {
        private float m_currentAngle;
        private float m_camber;
        private float m_caster;
        private Transform m_rotatingPart;
        
        public VisualWheelComponent(CarDesc.WheelInfo wheelInfo)
        {
            m_camber = wheelInfo.camber;
            m_caster = wheelInfo.caster;
        }

        public void ApplyVisuals(Transform wheelRotationPart, Transform wheelVisual, Transform wheelRoot, float angularVelocity, float currentLength, float steerAngle, bool isOppositeSide)
        {
            m_currentAngle += angularVelocity * Mathf.Rad2Deg * Time.fixedDeltaTime;
            m_currentAngle %= 360f;
            
            wheelVisual.position = wheelRoot.position - wheelRoot.up * currentLength;
            wheelRotationPart.localRotation = Quaternion.Euler(isOppositeSide ? -m_currentAngle : m_currentAngle, 0f, 0f);
            wheelVisual.localRotation = Quaternion.Euler(0f, isOppositeSide ? steerAngle + 180f : steerAngle, 0f);
        }
    }
}
