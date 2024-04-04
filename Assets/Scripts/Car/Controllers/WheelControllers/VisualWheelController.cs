using Car.Models.WheelModels;

namespace Car.Controllers.WheelControllers
{
    public class VisualWheelController : ICarController
    {
        private VisualWheelModel m_visualWheelModel;
        
        public void OnUpdate()
        {
            throw new System.NotImplementedException();
        }

        private void UpdateVisual()
        {
            //m_visualWheelModel.ApplyVisuals();
        }
    }
}
