using Car.Data;

namespace Car.Models.PhysicsModels
{
    public class AntirollBarModel
    {
        private bool m_isAntirollBarEnabled = true;
        private float m_stiffnessFront;
        private float m_stiffnessRear;
        private float[] m_lengthDifference = new float[2];
        private float[] m_force = new float[2];

        public AntirollBarModel(CarDesc.AntirollBarInfo antirollBarInfo)
        {
            m_isAntirollBarEnabled = antirollBarInfo.isEnabled;
            m_stiffnessFront = antirollBarInfo.stiffnessFront;
            m_stiffnessRear = antirollBarInfo.stiffnessRear;
        }
    }
}
