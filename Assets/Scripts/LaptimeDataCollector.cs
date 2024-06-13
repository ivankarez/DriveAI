using System.Text;
using UnityEngine;

public class LaptimeDataCollector : MonoBehaviour
{
    private Racetrack racetrack;
    private Vehicle vehicle;

    public float Laptime { get; private set; } = 0f;
    public float Sector1Time { get; private set; } = 0f;
    public float Sector2Time { get; private set; } = 0f;
    public float Sector3Time { get; private set; } = 0f;
    public float TopSpeed { get; private set; } = 0f;
    public float DistanceTravelled { get; private set; } = 0f;
    public float AverageSpeed { get; private set; } = 0f;
    public Signal<int> LapCount { get; private set; }
    public Signal<bool> IsDnf { get; private set; }

    private Vector3 lastPosition;
    private Vector3 nextWaypoint;

    public void Initialize(TrackPositionProvider trackPositionProvider, Vehicle vehicle, Racetrack racetrack)
    {
        this.vehicle = vehicle;
        this.racetrack = racetrack;

        LapCount = new Signal<int>(0);
        IsDnf = new Signal<bool>(false);
        lastPosition = vehicle.transform.position;
        trackPositionProvider.CurrentTrackPosition.OnChanged += OnTrackPositionChanged;
    }

    public void DoUpdate()
    {
        Laptime += Time.deltaTime;
        TopSpeed = Mathf.Max(TopSpeed, vehicle.speed);
        DistanceTravelled += Vector3.Distance(lastPosition, vehicle.transform.position);
        lastPosition = vehicle.transform.position;
        AverageSpeed = DistanceTravelled / Laptime * 3.6f;

        var waypointDistance = Vector3.Distance(vehicle.transform.position, nextWaypoint);
        if (waypointDistance > 10f)
        {
            IsDnf.Value = true;
        }
    }

    private void OnTrackPositionChanged(int oldValue, int newValue)
    {
        nextWaypoint = racetrack.TrackData.centerLine.CircularIndex(newValue);

        if (newValue == racetrack.Sector1StartIndex)
        {
            Sector3Time = Laptime - Sector2Time - Sector1Time;
            OnLapEnded();
            LapCount.Value++;
            OnLapStarted();
        }
        if (newValue == racetrack.Sector2StartIndex)
        {
            Sector1Time = Laptime;
        }
        if (newValue == racetrack.Sector3StartIndex)
        {
            Sector2Time = Laptime - Sector1Time;
        }
    }
    
    private void OnLapEnded()
    {
        var sb = new StringBuilder();
        sb.AppendLine($"Reference run laptime: {Laptime}");
        sb.AppendLine($"Sector 1 Time: {Sector1Time}");
        sb.AppendLine($"Sector 2 Time: {Sector2Time}");
        sb.AppendLine($"Sector 3 Time: {Sector3Time}");
        sb.AppendLine($"Top Speed: {TopSpeed}");
        sb.AppendLine($"Distance Travelled: {DistanceTravelled}");
        sb.AppendLine($"Average Speed: {AverageSpeed}");
        Debug.Log(sb.ToString());
    }

    private void OnLapStarted()
    {
        Laptime = 0f;
        TopSpeed = 0f;
        DistanceTravelled = 0f;
        AverageSpeed = 0f;
    }
}
