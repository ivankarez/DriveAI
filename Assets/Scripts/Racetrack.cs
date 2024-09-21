using Ivankarez.RacetrackGenerator;
using UnityEngine;

namespace Ivankarez.DriveAI
{
    public class Racetrack : MonoBehaviour
    {
        [SerializeField] private TrackData trackData;
        [SerializeField] private int spawnIndex;
        [SerializeField] private int sector1StartIndex;
        [SerializeField] private int sector2StartIndex;
        [SerializeField] private int sector3StartIndex;

        private void Awake()
        {
            RacingLine = new TrackLine(trackData.racingLine);
            CenterLine = new TrackLine(trackData.centerLine);
        }

        public void MoveToStart(Transform transform)
        {
            var spawnIndex = Random.Range(0, trackData.racingLine.Length);
            var position = trackData.racingLine.CircularIndex(spawnIndex);
            var rotation = Quaternion.LookRotation(trackData.racingLine.CircularIndex(spawnIndex + 1) - position);
            transform.SetPositionAndRotation(position, rotation);
        }

        public TrackData TrackData => trackData;
        public int SpawnIndex => spawnIndex;
        public int Sector1StartIndex => sector1StartIndex;
        public int Sector2StartIndex => sector2StartIndex;
        public int Sector3StartIndex => sector3StartIndex;
        public TrackLine RacingLine { get; private set; }
        public TrackLine CenterLine { get; private set; }
    }

    public class TrackLine
    {
        [SerializeField] private Vector3[] points;
        [SerializeField] private float[] angles;

        public TrackLine(Vector3[] points)
        {
            this.points = points;

            angles = new float[points.Length];
            for (var i = 0; i < points.Length; i++)
            {
                var dirToPrevPoint = (points.CircularIndex(i) - points.CircularIndex(i - 1)).normalized;
                var dirToNextPoint = (points.CircularIndex(i + 1) - points.CircularIndex(i)).normalized;
                angles[i] = Vector3.SignedAngle(dirToPrevPoint, dirToNextPoint, Vector3.up);
            }
        }

        public int FindClosestPoint(Vector3 position)
        {
            var closestPoint = 0;
            var closestDistance = float.MaxValue;

            for (var i = 0; i < points.Length; i++)
            {
                var lineDistance = DistanceFromLinePoint(i, position);
                var distance = Vector3.Distance(points.CircularIndex(i), position);
                if (distance < 10 && lineDistance < 0 && -lineDistance < closestDistance) // Only consider points before the vehicle
                {
                    closestDistance = lineDistance;
                    closestPoint = i;
                }
            }

            return closestPoint;
        }

        public bool IsPointReached(int point, Vector3 position)
        {
            return DistanceFromLinePoint(point, position) > 0;
        }

        public int GetNextPoint(int point)
        {
            return points.CalculateCircularIndex(point + 1);
        }

        public float DistanceFromLinePoint(int point, Vector3 position)
        {
            var pointPosition = points.CircularIndex(point);
            var nextPointPosition = points.CircularIndex(point + 1);
            var racingLineDirection = (nextPointPosition - pointPosition).normalized;
            var positionDelta = position - pointPosition;
            return Vector3.Dot(racingLineDirection, positionDelta);
        }

        public Vector3 GetPoint(int racingLinePositionIndex)
        {
            return points.CircularIndex(racingLinePositionIndex);
        }

        public float GetAngle(int racingLinePositionIndex)
        {
            return angles.CircularIndex(racingLinePositionIndex);
        }
    }
}
