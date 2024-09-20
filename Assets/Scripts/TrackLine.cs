using UnityEngine;

public class TrackLineOld
{
    public Vector3[] Line { get; private set; }
    public float[] Angles { get; private set; }
    public Signal<int> CurrentIndex { get; private set; }
    public float DistanceFromNextPoint { get; private set; }
    public Signal<Vector3> CurrentPoint { get; private set; }
    public Signal<Vector3> VehiclePosition { get; set; }

    public TrackLineOld(Vector3[] line, Vector3 initialPosition)
    {
        Line = line;
        VehiclePosition = new Signal<Vector3>(initialPosition);
        CurrentIndex = new Signal<int>(line.CalculateCircularIndex(FindClosestPoint(initialPosition)));
        DistanceFromNextPoint = DistanceFromLinePoint(CurrentIndex.Value, initialPosition);
        CurrentPoint = new Signal<Vector3>(Line[CurrentIndex.Value]);

        Angles = new float[line.Length];
        for (var i = 0; i < line.Length; i++)
        {
            var dirToPrevPoint = (Line.CircularIndex(i) - Line.CircularIndex(i - 1)).normalized;
            var dirToNextPoint = (Line.CircularIndex(i + 1) - Line.CircularIndex(i)).normalized;
            Angles[i] = Vector3.SignedAngle(dirToPrevPoint, dirToNextPoint, Vector3.up);
        }

        CurrentIndex.OnChanged += (_, value) => CurrentPoint.Value = Line[value];
        VehiclePosition.OnChanged += Update;
    }

    private int FindClosestPoint(Vector3 position)
    {
        var closestPoint = 0;
        var closestDistance = float.MaxValue;

        for (var i = 0; i < Line.Length; i++)
        {
            var distance = DistanceFromLinePoint(i, position);
            if (distance < 0 && -distance < closestDistance) // Only consider points before the vehicle
            {
                closestDistance = distance;
                closestPoint = i;
            }
        }

        return closestPoint;
    }

    private void Update(Vector3 oldPosition, Vector3 newPosition)
    {
        DistanceFromNextPoint = DistanceFromLinePoint(CurrentIndex.Value, newPosition);

        if (DistanceFromNextPoint > 0)
        {
            CurrentIndex.Value = Line.CalculateCircularIndex(CurrentIndex.Value + 1);
        }
    }

    private float DistanceFromLinePoint(int point, Vector3 position)
    {
        var pointPosition = Line.CircularIndex(point);
        var nextPointPosition = Line.CircularIndex(point + 1);
        var racingLineDirection = (nextPointPosition - pointPosition).normalized;
        var positionDelta = position - pointPosition;
        return Vector3.Dot(racingLineDirection, positionDelta);
    }
}
