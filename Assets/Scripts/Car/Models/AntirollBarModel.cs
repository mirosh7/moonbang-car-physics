namespace Car.Models
{
    public class AntirollBarModel
    {
        private bool m_isAntirollBarEnabled = true;
        private float m_stiffnessFront;
        private float m_stiffnessRear;
        private float[] m_lengthDifference = new float[2];
        private float[] m_force = new float[2];

        public AntirollBarModel(bool isAntirollBarEnabled, float stiffnessFront, float stiffnessRear)
        {
            m_isAntirollBarEnabled = isAntirollBarEnabled;
            m_stiffnessFront = stiffnessFront;
            m_stiffnessRear = stiffnessRear;
        }
    }
}
