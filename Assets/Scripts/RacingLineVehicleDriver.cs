using System.Linq;
using UnityEngine;

public class RacingLineVehicleDriver : DriverBase
{
    [SerializeField] private float caution = 15f;
    [SerializeField] private AnimationCurve cautionOverSpeed;

    private float steering = 0f;
    private float throttle = 0f;
    private float brake = 0f;
    private int currentLookAhead = 0;

    protected override Actions UpdateDriver(Observations observations)
    {
        steering = Mathf.Clamp(observations.VehicleAngleToTrack / vehicle.maxSteerAngle, -1, 1);

        var suggestedSpeed = CalculateSuggestedSpeed(observations);
        var relativeSpeed = suggestedSpeed - vehicle.speed;
        throttle = relativeSpeed > 0f ? 1f : 0f;

        brake = relativeSpeed < 0f ? Mathf.Clamp(Mathf.Abs(relativeSpeed) / 1f, .3f, 1f) : 0f;

        return new Actions(steering, throttle, brake);
    }

    private float CalculateSuggestedSpeed(Observations observations)
    {
        var topSpeedLookAhead = observations.Waypoints.Count - 2;
        var speedRange = Mathf.InverseLerp(0, vehicle.maxSpeed, vehicle.speed);
        currentLookAhead = Mathf.RoundToInt(Mathf.Lerp(1, topSpeedLookAhead, speedRange));
        var maxAngle = observations.WaypointAngles.Take(currentLookAhead).Select(Mathf.Abs).Max();

        var currentCaution = cautionOverSpeed.Evaluate(speedRange) * caution;
        return vehicle.maxSpeed - ((maxAngle / 180f) * vehicle.maxSpeed * currentCaution);
    }

    private void OnGUI()
    {
        GUI.Label(new Rect(10, 10, 200, 20), "Speed: " + vehicle.speed.ToString("0.0") + " km/h");
        GUI.Label(new Rect(10, 30, 200, 20), "Throttle: " + throttle.ToString("0.0"));
        GUI.Label(new Rect(10, 50, 200, 20), "Brake: " + brake.ToString("0.0"));
        GUI.Label(new Rect(10, 70, 200, 20), "Steering: " + steering.ToString("0.0"));
        GUI.Label(new Rect(10, 90, 200, 20), "ABS: " + vehicle.isAbsActive.Any(b => b));
        GUI.Label(new Rect(10, 110, 400, 20), "Slip (FWD): " + string.Join(", ", vehicle.wheelHits.Where(h => h != null).Select(h => $"{h.Value.forwardSlip:f2}")));
        GUI.Label(new Rect(10, 130, 400, 20), "Slip (SDE): " + string.Join(", ", vehicle.wheelHits.Where(h => h != null).Select(h => $"{h.Value.sidewaysSlip:f2}")));
    }
}
