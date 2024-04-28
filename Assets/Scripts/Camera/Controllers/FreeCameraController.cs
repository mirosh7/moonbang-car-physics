using Camera.Models;
using Cinemachine;
using UnityEngine;

namespace Camera.Controllers
{
    public class FreeCameraController : IController
    {
        private FreeCameraModel m_freeCameraModel;

        public FreeCameraController(FreeCameraModel freeCameraModel, Transform carTransform)
        {
            m_freeCameraModel = freeCameraModel;
            m_freeCameraModel.AttachCameraTo(carTransform);
        }

        public void OnUpdate()
        {
            UpdateFreeCamera();
        }

        private void UpdateFreeCamera()
        {
            m_freeCameraModel.UpdateCamera();
        }
    }
}
