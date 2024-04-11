using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UICarInputInfo : MonoBehaviour
{
    [SerializeField]
    private List<TMP_Text> m_tmpTexts;

    public void SetSteering(float data)
    {
        m_tmpTexts[0].text = $"Steer = {data}";
    }

    public void SetThrottle(float data)
    {
        m_tmpTexts[1].text = $"Throttle = {data}";
    }

    public void SetBrakes(float data)
    {
        m_tmpTexts[2].text = $"Brakes = {data}";
    }

    public void SetClutch(float data)
    {
        m_tmpTexts[3].text = $"Clutch = {data}";
    }

    public void SetHandBrake(float data)
    {
        m_tmpTexts[4].text = $"Hand Brake = {data}";
    }
}
