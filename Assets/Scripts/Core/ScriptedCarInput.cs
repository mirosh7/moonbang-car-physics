using System;

public class ScriptedCarInput : ICarInput
{
    public float acceleration { get; set; }
    public float brakes { get; set; }
    public float steering { get; set; }
    public float clutch { get; set; }
    public float handbrake { get; set; }
    public bool blockCar { get; set; }

    public Action gearUp { get; set; }
    public Action gearDown { get; set; }

    public void RequestGearUp() => gearUp?.Invoke();
    public void RequestGearDown() => gearDown?.Invoke();
}
