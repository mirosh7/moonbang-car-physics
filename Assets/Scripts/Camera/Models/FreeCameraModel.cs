using Cinemachine;
using UnityEngine;

namespace Camera.Models
{
    public class FreeCameraModel
    {
        private CinemachineFreeLook m_cinemachineFreeLookCamera;
        private InputManager m_inputManager;

        public FreeCameraModel(CinemachineFreeLook cinemachineFreeLookCamera, InputManager inputManager)
        {
            m_cinemachineFreeLookCamera = cinemachineFreeLookCamera;
            m_inputManager = inputManager;
        }

        public void UpdateCamera()
        {
            UpdateCameraInput();
        }

        public void AttachCameraTo(Transform transform)
        {
            m_cinemachineFreeLookCamera.LookAt = transform;
            m_cinemachineFreeLookCamera.Follow = transform;
        }

        private void UpdateCameraInput()
        {
            m_cinemachineFreeLookCamera.m_XAxis.m_InputAxisValue = m_inputManager.rotation.x;
            m_cinemachineFreeLookCamera.m_YAxis.m_InputAxisValue = m_inputManager.rotation.y;
        }
    }
}
