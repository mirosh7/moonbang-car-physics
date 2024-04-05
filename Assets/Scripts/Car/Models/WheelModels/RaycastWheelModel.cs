using Car.Data;
using UnityEngine;

namespace Car.Models.WheelModels
{
    public class RaycastWheelModel
    {
        private RaycastHit m_wheelHit;
        private bool m_isWheelHit;
        private LayerMask m_raycastLayer;
        private float m_wheelRadius;
        private float m_restLength;

        public RaycastWheelModel(CarDesc.WheelInfo wheelInfo)
        {
            m_wheelRadius = wheelInfo.wheelRadius;
            m_restLength = wheelInfo.restLength;
            m_raycastLayer = wheelInfo.raycastLayer;
        }
        
        public bool isWheelHit => m_isWheelHit;

        public RaycastHit wheelHit => m_wheelHit;

        private void Raycast(Transform wheelRoot)
        {
            m_isWheelHit = Physics.Raycast(wheelRoot.position,
                -wheelRoot.up,
                out m_wheelHit,
                (m_restLength + m_wheelRadius),
                m_raycastLayer);
        }
    }
}
