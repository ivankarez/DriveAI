using UnityEngine;

public class VehicleSpawner : MonoBehaviour
{
    [SerializeField] private Vehicle vehiclePrefab;
    [SerializeField] private Racetrack raceTrack;

    public Vehicle SpawnVehicle()
    {
        var vehicle = Instantiate(vehiclePrefab);
        var trackData = raceTrack.TrackData;
        var trackIndex = raceTrack.SpawnIndex;
        vehicle.transform.SetPositionAndRotation(trackData.racingLine.CircularIndex(trackIndex), Quaternion.LookRotation(trackData.racingLine.CircularIndex(trackIndex + 1) - trackData.racingLine.CircularIndex(trackIndex)));
        vehicle.transform.position += vehicle.transform.TransformDirection(Vector3.back) * (vehicle.nose.localPosition.z + 0.1f);

        return vehicle;
    }
}
