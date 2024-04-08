
using UnityEngine;

public class InputManager
{
    public InputManager()
    {
        
    }
    
    private float m_throttle = 1f;
    public float throttle => m_throttle;

    private float m_brakes = 1f;
    public float brakes => m_brakes;

    private float m_steering = 1f;
    public float steering => m_steering;
}

