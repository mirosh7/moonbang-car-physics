using System;
using UnityEngine;
using UnityEngine.UI;

public class UICarInputView : MonoBehaviour
{
    [SerializeField] private Image m_throttle;
    [SerializeField] private Image m_brake;
    [SerializeField] private Image m_clutch;
    [SerializeField] private Image m_handbrake;
    
    [SerializeField] private InputManager m_inputManager;

    private void Update()
    {
        m_throttle.fillAmount = m_inputManager.acceleration;
        m_brake.fillAmount = m_inputManager.brakes;
        m_clutch.fillAmount = m_inputManager.clutch;
        m_handbrake.fillAmount = m_inputManager.handbrake;
    }
}
