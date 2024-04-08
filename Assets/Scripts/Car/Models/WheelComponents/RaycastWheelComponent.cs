using Car.Data;
using UnityEngine;

namespace Car.Models.WheelComponents
{
    public class RaycastWheelComponent
    {
        private RaycastHit m_wheelHit;
        private bool m_isWheelHit;
        private LayerMask m_raycastLayer;
        private float m_wheelRadius;
        private float m_restLength;

        public RaycastWheelComponent(CarDesc.WheelInfo wheelInfo)
        {
            m_wheelRadius = wheelInfo.wheelRadius;
            m_restLength = wheelInfo.restLength;
            m_raycastLayer = wheelInfo.raycastLayer;
        }
        
        public bool isWheelHit => m_isWheelHit;

        public RaycastHit wheelHit => m_wheelHit;

        public void UpdateRaycast(Transform wheelRoot)
        {
            var position = wheelRoot.position;
            m_isWheelHit = Physics.Raycast(position,
                -wheelRoot.up,
                out m_wheelHit,
                (m_restLength + m_wheelRadius),
                m_raycastLayer);
            
            Debug.DrawRay(position, Vector3.down * (m_restLength + m_wheelRadius), Color.red);
        }
    }
}
