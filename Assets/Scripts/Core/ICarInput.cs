using System;

public interface ICarInput
{
    float acceleration { get; }
    float brakes { get; }
    float steering { get; }
    float clutch { get; }
    float handbrake { get; }
    bool blockCar { get; }

    Action gearUp { get; set; }
    Action gearDown { get; set; }
}
