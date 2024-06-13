using System.Collections.Generic;
using UnityEngine;

public class ObservationsCollector : MonoBehaviour
{
    private Vehicle vehicle = null;
    private Racetrack racetrack = null;

    [SerializeField] private int lookAhead = 30;

    public TrackLine RacingLine { get; private set; }
    public List<Vector3> Waypoints { get; private set; }
    public List<float> WaypointAngles { get; private set; }
    public float WaypointAngle { get; private set; }
    public Vehicle Vehicle => vehicle;

    public void Initialize(Vehicle vehicle, Racetrack racetrack)
    {
        this.vehicle = vehicle;
        this.racetrack = racetrack;
        RacingLine = new TrackLine(this.racetrack.TrackData.racingLine, vehicle.nose.transform.position);
        Waypoints = new List<Vector3>(lookAhead);
        WaypointAngles = new List<float>(lookAhead);
    }

    private void Start()
    {
        for (var i = 0; i < lookAhead; i++)
        {
            var index = racetrack.SpawnIndex + i;
            Waypoints.Add(RacingLine.Line.CircularIndex(index));
            WaypointAngles.Add(RacingLine.Angles.CircularIndex(index));
        }

        RacingLine.CurrentIndex.OnChanged += UpdateWaypoints;
    }

    private void UpdateWaypoints(int oldValue, int newValue)
    {
        Waypoints.RemoveAt(0);
        WaypointAngles.RemoveAt(0);

        var nextIndex = newValue + lookAhead - 1;
        Waypoints.Add(RacingLine.Line.CircularIndex(nextIndex));
        WaypointAngles.Add(RacingLine.Angles.CircularIndex(nextIndex));
    }

    public Observations CollectObservations()
    {
        RacingLine.VehiclePosition.Value = vehicle.nose.transform.position;

        WaypointAngle = CalculateWaypointAngle();

        return new Observations(Waypoints, WaypointAngles, vehicle.speed, WaypointAngle);
    }

    private float CalculateWaypointAngle()
    {
        var dirToRacingLine = RacingLine.CurrentPoint.Value - vehicle.transform.position;
        return Vector3.SignedAngle(vehicle.transform.forward, dirToRacingLine, Vector3.up);
    }
}
