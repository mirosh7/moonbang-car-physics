using Car.Data;
using UnityEngine;

namespace Car.Models.PhysicsModels.WheelComponents
{
    public class RaycastWheelComponent
    {
        private CarDesc.WheelInfo m_wheelInfo;
        private RaycastHit m_wheelHit;
        private bool m_isWheelHit;

        public RaycastWheelComponent(CarDesc.WheelInfo wheelInfo)
        {
            m_wheelInfo = wheelInfo;
        }
        
        public bool isWheelHit => m_isWheelHit;

        public RaycastHit wheelHit => m_wheelHit;

        public void UpdateRaycast(Transform wheelRoot)
        {
            var position = wheelRoot.position;
            m_isWheelHit = Physics.Raycast(position,
                -wheelRoot.up,
                out m_wheelHit,
                (m_wheelInfo.restLength + m_wheelInfo.wheelRadius),
                m_wheelInfo.raycastLayer);
            
            Debug.DrawRay(position, Vector3.down * (m_wheelInfo.restLength + m_wheelInfo.wheelRadius), Color.red);
        }
    }
}
