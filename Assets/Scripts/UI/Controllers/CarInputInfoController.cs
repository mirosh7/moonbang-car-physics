using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarInputInfoController : IController
{
    private UICarInputInfo m_inputInfo;
    private InputManager m_inputManager;

    public CarInputInfoController(UICarInputInfo inputInfo, InputManager inputManager)
    {
        m_inputInfo = inputInfo;
        m_inputManager = inputManager;
    }

    public void OnUpdate()
    {
        UpdateCarInputInfo();
    }

    private void UpdateCarInputInfo()
    {
        m_inputInfo.SetSteering(m_inputManager.steering);
        m_inputInfo.SetThrottle(m_inputManager.acceleration);
        m_inputInfo.SetBrakes(m_inputManager.brakes);
        m_inputInfo.SetClutch(0f); //TODO
        m_inputInfo.SetHandBrake(0f);
    }
}
