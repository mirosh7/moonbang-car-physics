using UnityEngine;

namespace Car.Models
{
    public class CarPrefabLoadModel
    {
        private const string CAR_CORE_NAME = "CarCore";
        private string m_carPrefabName;
        private string m_carWheelName;

        private GameObject m_carCore;
        private GameObject m_carVisualPrefab;
        private GameObject m_carWheel;

        public GameObject carVisualPrefab => m_carVisualPrefab;
        public GameObject carWheel => m_carWheel;
        public GameObject carCore => m_carCore;

        public CarPrefabLoadModel(string carPrefabName, string carWheelName)
        {
            m_carPrefabName = carPrefabName;
            m_carWheelName = carWheelName;
        }

        public void LoadPrefabs()
        {
            m_carVisualPrefab = Resources.Load<GameObject>(m_carPrefabName);
            m_carWheel = Resources.Load<GameObject>(m_carWheelName);
            m_carCore = Resources.Load<GameObject>(CAR_CORE_NAME);
        }
    }
}
