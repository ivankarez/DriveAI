using Ivankarez.NeuralNetworks;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Ivankarez.DriveAI
{
    public class AiVehicleController : MonoBehaviour
    {
        private LayeredNetworkModel neuralNetwork;
        private Action onTrackLeft;
        private float[] inputs;
        private Racetrack racetrack;
        private int racingLinePositionIndex;
        private int centerLinePositionIndex;

        [SerializeField] private Lidar lidar;
        [SerializeField] private Vehicle vehicle;
        [SerializeField] private LayerMask racetrackLayer;
        [SerializeField] private bool isAiSteering;
        [SerializeField] private bool isAiThrottle;
        [SerializeField] private int lookAhead = 30;
        [SerializeField] private float caution = 15f;
        [SerializeField] private AnimationCurve cautionOverSpeed;

        private float throttle = 0f;
        private float brake = 0f;
        private float steering = 0f;
        private List<float> waypointAngles;
        private int currentLookAhead;
        private float curvature;
        private Vector3 lastPosition;
        private float timeOffRoad = 0f;

        public float CheckpointsReached { get; private set; }
        public float DistnaceTravelled { get; private set; }
        public float Speed => vehicle.speed;
        public Vehicle Vehicle => vehicle;

        private void Awake()
        {
            racetrack = FindObjectOfType<Racetrack>();
        }

        private void Start()
        {
            lidar.RaysCount = DnaUtils.InputSize - 6;
            inputs = new float[DnaUtils.InputSize];
        }

        public void Initialize(LayeredNetworkModel neuralNetwork, Action onTrackLeft)
        {
            this.neuralNetwork = neuralNetwork;
            this.onTrackLeft = onTrackLeft;
            racingLinePositionIndex = racetrack.RacingLine.FindClosestPoint(transform.position);
            centerLinePositionIndex = racetrack.CenterLine.FindClosestPoint(transform.position);

            waypointAngles = new List<float>(lookAhead);
            for (var i = 0; i < lookAhead; i++)
            {
                waypointAngles.Add(racetrack.RacingLine.GetAngle(racingLinePositionIndex + i));
            }
        }

        private void Update()
        {
            if (neuralNetwork == null)
            {
                vehicle.SetInputs(0, 0, 1f);
                return;
            }

            UpdatePositions();

            UpdateAiDriving();
            UpdateAlgorithmicDriving();

            vehicle.SetInputs(steering, throttle, brake);

            CheckTrackLeft();

            DistnaceTravelled += Vector3.Distance(lastPosition, transform.position);
            lastPosition = transform.position;
        }

        private void UpdatePositions()
        {
            var positionPoint = transform.position + vehicle.transform.forward * 4;

            if (racetrack.RacingLine.IsPointReached(racingLinePositionIndex, positionPoint))
            {
                racingLinePositionIndex = racetrack.RacingLine.GetNextPoint(racingLinePositionIndex);
                waypointAngles.RemoveAt(0);
                waypointAngles.Add(racetrack.RacingLine.GetAngle(racingLinePositionIndex + lookAhead - 1));
            }

            if (racetrack.CenterLine.IsPointReached(centerLinePositionIndex, positionPoint))
            {
                centerLinePositionIndex = racetrack.CenterLine.GetNextPoint(centerLinePositionIndex);
                CheckpointsReached += 1;
                curvature = CalculateTrackCurvature();
            }
        }

        private void UpdateAiDriving()
        {
            lidar.GetDistances(inputs);

            var distFromRacingLine = DistanceFromTrackLine(racetrack.RacingLine, racingLinePositionIndex);
            var distFromCenterLine = DistanceFromTrackLine(racetrack.CenterLine, centerLinePositionIndex);
            inputs[^6] = AngleFromTrackLine(racetrack.RacingLine, racingLinePositionIndex) / 20f;
            inputs[^5] = AngleFromTrackLine(racetrack.CenterLine, centerLinePositionIndex) / 20f;
            inputs[^4] = vehicle.speed / 100f;
            inputs[^3] = curvature / (180 / lookAhead);
            inputs[^2] = distFromRacingLine / 6;
            inputs[^1] = distFromCenterLine / 6;

            var outputs = neuralNetwork.Feedforward(inputs);

            throttle = Mathf.Clamp(outputs[0], 0, 1);
            brake = Mathf.Clamp(outputs[0] * -1, 0, 1);
            steering = Mathf.Clamp(outputs[1], -1, 1);
        }

        private void UpdateAlgorithmicDriving()
        {
            if (!isAiSteering)
            {
                var dirToRacingLine = racetrack.RacingLine.GetPoint(racingLinePositionIndex) - vehicle.transform.position;
                var vehicleAngleToTrack = Vector3.SignedAngle(vehicle.transform.forward, dirToRacingLine, Vector3.up);
                steering = Mathf.Clamp(vehicleAngleToTrack / vehicle.maxSteerAngle, -1, 1);
            }

            if (!isAiThrottle)
            {
                var suggestedSpeed = CalculateSuggestedSpeed();
                var relativeSpeed = suggestedSpeed - vehicle.speed;
                throttle = relativeSpeed > 0f ? 1f : 0f;
                brake = relativeSpeed < 0f ? Mathf.Clamp(Mathf.Abs(relativeSpeed) / 1f, .3f, 1f) : 0f;
            }
        }

        private float CalculateSuggestedSpeed()
        {
            var topSpeedLookAhead = lookAhead - 2;
            var speedRange = Mathf.InverseLerp(0, vehicle.maxSpeed, vehicle.speed);
            currentLookAhead = Mathf.RoundToInt(Mathf.Lerp(1, topSpeedLookAhead, speedRange));
            var maxAngle = waypointAngles.Take(currentLookAhead).Select(Mathf.Abs).Max();

            var currentCaution = cautionOverSpeed.Evaluate(speedRange) * caution;
            return vehicle.maxSpeed - ((maxAngle / 180f) * vehicle.maxSpeed * currentCaution);
        }

        private void CheckTrackLeft()
        {
            if (vehicle.wheelHits.All((hit) => !IsOnTrack(hit)))
            {
                timeOffRoad += Time.deltaTime;
            }
            else
            {
                timeOffRoad = 0f;
            }

            if (timeOffRoad > 1f)
            {
                onTrackLeft?.Invoke();
                vehicle.SetInputs(0, 0, 1f);
                enabled = false;
            }
        }

        private bool IsOnTrack(WheelHit? hit)
        {
            if (hit.HasValue)
            {
                var collider = hit.Value.collider;
                return IsLayerOnMask(collider.gameObject.layer, racetrackLayer);
            }

            return false;
        }

        private bool IsLayerOnMask(int layer, LayerMask mask)
        {
            return mask == (mask | (1 << layer));
        }

        private float DistanceFromTrackLine(TrackLine line, int currentIndex)
        {
            var currentPoint = line.GetPoint(currentIndex);
            var currentPoint2D = new Vector2(currentPoint.x, currentPoint.z);
            var prevPoint = line.GetPoint(currentIndex - 1);
            var prevPoint2D = new Vector2(prevPoint.x, prevPoint.z);
            var position2D = new Vector2(vehicle.transform.position.x, vehicle.transform.position.z);

            return AdvMath.DistanceFromPointToLine2D(prevPoint2D, currentPoint2D, position2D);
        }

        private float AngleFromTrackLine(TrackLine line, int currentIndex)
        {
            var currentPoint = line.GetPoint(currentIndex);
            var prevPoint = line.GetPoint(currentIndex - 1);

            return Vector3.SignedAngle(currentPoint - prevPoint, transform.forward, Vector3.up);
        }

        private float CalculateTrackCurvature()
        {
            var curvature = 0f;
            for (var i = 0; i < lookAhead; i++)
            {
                curvature += Mathf.Abs(racetrack.RacingLine.GetAngle(racingLinePositionIndex + i));
            }

            return curvature / lookAhead;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(racetrack.RacingLine.GetPoint(racingLinePositionIndex), 0.1f);
            Gizmos.color = Color.white;
            Gizmos.DrawSphere(racetrack.CenterLine.GetPoint(centerLinePositionIndex), 0.1f);
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(transform.position, racetrack.CenterLine.GetPoint(centerLinePositionIndex));
            Gizmos.DrawLine(transform.position, racetrack.RacingLine.GetPoint(racingLinePositionIndex));
        }
    }
}
