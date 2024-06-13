using Ivankarez.RacetrackGenerator;
using UnityEngine;

public class Racetrack : MonoBehaviour
{
    [SerializeField] private TrackData trackData;
    [SerializeField] private int spawnIndex;
    [SerializeField] private int sector1StartIndex;
    [SerializeField] private int sector2StartIndex;
    [SerializeField] private int sector3StartIndex;

    public Vector3 GetSpawnPosition()
    {
        return trackData.racingLine.CircularIndex(spawnIndex);
    }

    public TrackData TrackData => trackData;
    public int SpawnIndex => spawnIndex;
    public int Sector1StartIndex => sector1StartIndex;
    public int Sector2StartIndex => sector2StartIndex;
    public int Sector3StartIndex => sector3StartIndex;
}
