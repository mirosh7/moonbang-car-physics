using System.Collections.Generic;
using Car.Data;
using UnityEngine;

public class CarFactory : MonoBehaviour
{
    [SerializeField]
    private CarDesc m_carDesc;

    [SerializeField]
    private Transform m_carParent;
    
    private void Start()
    {
        var carBuilder = new CarBuilder(m_carDesc, "Mazda", "Wheel");
        carBuilder.BuildCar(m_carParent);
    }

}