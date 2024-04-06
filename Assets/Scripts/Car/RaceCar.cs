using System;
using System.Collections.Generic;
using Car.Data;
using UnityEngine;

namespace Car
{
    public class RaceCar : MonoBehaviour
    {
        [SerializeField]
        private CarDesc m_carDesc;
        [SerializeField]
        private Rigidbody m_rb;
        [SerializeField]
        private List<Transform> m_wheelTransforms;
        [SerializeField]
        private List<Transform> m_wheelRootTransforms;
        private void Start()
        {
            var carBuilder = new CarBuilder(m_carDesc, m_wheelTransforms, m_wheelRootTransforms, m_rb);
            carBuilder.Build();
        }
    }
}
