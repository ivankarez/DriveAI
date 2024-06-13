using System;
using UnityEngine;

[Serializable]
public class Actions
{
    [SerializeField] private float steering;
    [SerializeField] private float throttle;
    [SerializeField] private float brake;

    public float Steering => steering;
    public float Throttle => throttle;
    public float Brake => brake;

    public Actions(float steering, float throttle, float brake)
    {
        this.steering = steering;
        this.throttle = throttle;
        this.brake = brake;
    }

    internal float CalculateError(float[] output)
    {
        var steeringError = Math.Abs(output[0] - Steering);
        var throttleError = Math.Abs(output[1] - Throttle);
        var brakeError = Math.Abs(output[2] - Brake);

        return steeringError + throttleError + brakeError;
    }

    internal float[] ToArray()
    {
        return new[] { Steering, Throttle, Brake };
    }
}
