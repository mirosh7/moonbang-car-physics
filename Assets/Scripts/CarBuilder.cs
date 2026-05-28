using System.Collections.Generic;
using System.Linq;
using Car;
using Car.Controllers;
using Car.Data;
using Car.Models;
using UnityEngine;

public class CarBuilder
{
    private readonly CarDesc m_carDesc;
    private readonly string m_carPrefabName;
    private readonly string m_carWheelName;

    private RaceCar m_raceCar;
    public RaceCar raceCar => m_raceCar;

    public CarBuilder(CarDesc carDesc, string carPrefabName, string carWheelName)
    {
        m_carDesc = carDesc;
        m_carPrefabName = carPrefabName;
        m_carWheelName = carWheelName;
    }

    public RaceCar BuildCar(Transform spawnPoint)
    {
        var prefabs = new CarPrefabLoadModel(m_carPrefabName, m_carWheelName);
        var prefabLoader = new CarPrefabLoadController(prefabs);

        var carCore = Object.Instantiate(prefabLoader.carCore, spawnPoint.position, spawnPoint.rotation);
        var carVisual = Object.Instantiate(prefabLoader.carPrefab, carCore.transform);

        var wheelVisuals = new List<Transform>();
        foreach (var _ in m_carDesc.wheelInfos)
        {
            var wheel = Object.Instantiate(prefabLoader.wheelPrefab, carCore.transform);
            wheelVisuals.Add(wheel.transform);
        }

        var wheelRoots = carVisual.GetComponentsInChildren<Transform>()
                                  .Where(t => t.name == "wheelRoot")
                                  .ToList();

        m_raceCar = carCore.GetComponent<RaceCar>();
        var rb = carCore.GetComponent<Rigidbody>();

        m_raceCar.Initialize(m_carDesc, rb, wheelRoots, wheelVisuals, InputManager.instance);

        var engineSound = carCore.GetComponentInChildren<RealisticEngineSound>();
        if (engineSound != null) engineSound.car = m_raceCar;

        return m_raceCar;
    }
}
