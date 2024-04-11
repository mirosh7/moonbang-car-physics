using System.Collections.Generic;
using Car.Models;
using UnityEngine;

namespace Car.Controllers
{
    public class CarPrefabLoadController
    {
        private CarPrefabLoadModel m_carPrefabLoadModel;
        
        public CarPrefabLoadController(CarPrefabLoadModel carPrefabLoadModel)
        {
            m_carPrefabLoadModel = carPrefabLoadModel;
            m_carPrefabLoadModel.LoadPrefabs();
        }
        
        public GameObject carPrefab => m_carPrefabLoadModel.carVisualPrefab;
        public GameObject wheelPrefab => m_carPrefabLoadModel.carWheel;
        public GameObject carCore => m_carPrefabLoadModel.carCore;
    }
}
