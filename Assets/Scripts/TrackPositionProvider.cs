using UnityEngine;

public class TrackPositionProvider : MonoBehaviour
{
    private Transform trackedTransform = null;

    private TrackLineOld centerLine;

    public Signal<int> CurrentTrackPosition { get; private set; }

    public void Intialize(Racetrack racetrack, Transform trackedTransform)
    {
        this.trackedTransform = trackedTransform;

        centerLine = new TrackLineOld(racetrack.TrackData.centerLine, trackedTransform.position);
        CurrentTrackPosition = centerLine.CurrentIndex;
    }

    public void DoUpdate()
    {
        centerLine.VehiclePosition.Value = trackedTransform.position;
    }
}