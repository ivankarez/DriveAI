using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class Observations
{
    [SerializeField] private List<Vector3> waypoints;
    [SerializeField] private List<float> waypointAngles;
    [SerializeField] private float vehicleSpeed;
    [SerializeField] private float vehicleAngleToTrack;

    public IReadOnlyList<Vector3> Waypoints => waypoints;
    public IReadOnlyList<float> WaypointAngles => waypointAngles;
    public float VehicleSpeed => vehicleSpeed;
    public float VehicleAngleToTrack => vehicleAngleToTrack;

    public Observations(ICollection<Vector3> waypoints, ICollection<float> waypointAngles, float vehicleSpeed, float vehicleAngleToTrack)
    {
        this.waypoints = waypoints.ToList();
        this.waypointAngles = waypointAngles.ToList();
        this.vehicleSpeed = vehicleSpeed;
        this.vehicleAngleToTrack = vehicleAngleToTrack;
    }

    public void ToArray(float[] array)
    {
        for (int i = 0; i < WaypointAngles.Count; i++)
        {
            array[i] = WaypointAngles[i] / 15f;
        }
        array[^2] = VehicleAngleToTrack / 90f;
        array[^1] = VehicleSpeed / 340f;
    }

    public float[] ToArray()
    {
        var array = new float[WaypointAngles.Count + 2];
        ToArray(array);
        return array;
    }
}
