using UnityEngine;

namespace Ivankarez.DriveAI
{
    public class Lidar : MonoBehaviour
    {
        [SerializeField] private float maxDistance = 100f;
        [SerializeField][Range(15f, 180f)] private float fieldOfView = 90f;
        [SerializeField][Range(2, 11)] private int raysCount = 5;
        [SerializeField] private Gradient gizmoColor;
        [SerializeField] private LayerMask layerMask;

        private Vector3[] rayDirections;
        private float[] distances;

        private void Start()
        {
            rayDirections = new Vector3[raysCount];
            distances = new float[raysCount];
        }

        private void OnValidate()
        {
            if (raysCount < 2)
            {
                raysCount = 2;
                Debug.LogWarning("Rays count can't be less than 2");
            }

            if (rayDirections == null || rayDirections.Length != raysCount)
            {
                rayDirections = new Vector3[raysCount];
                distances = new float[raysCount];
            }
        }

        private void FixedUpdate()
        {
            transform.GetPositionAndRotation(out var position, out var rotation);
            var halfFov = fieldOfView / 2;
            var angleStep = fieldOfView / (raysCount - 1);

            for (var i = 0; i < raysCount; i++)
            {
                var angle = -halfFov + i * angleStep;
                var rayDirection = rotation * Quaternion.AngleAxis(angle, Vector3.up) * Vector3.forward;
                rayDirections[i] = rayDirection;

                if (Physics.Raycast(position, rayDirection, out var hit, maxDistance, layerMask))
                {
                    distances[i] = hit.distance;
                }
                else
                {
                    distances[i] = maxDistance;
                }
            }
        }

        private void OnDrawGizmos()
        {
            var position = transform.position;

            for (var i = 0; i < raysCount; i++)
            {
                var rayDirection = rayDirections[i];
                var distance = distances[i];

                Gizmos.color = gizmoColor.Evaluate(distance / maxDistance);
                Gizmos.DrawRay(position, rayDirection * distance);
            }
        }
        public int RaysCount
        {
            get => raysCount;
            set
            {
                raysCount = value;
                OnValidate();
            }
        }
        public void GetDistances(float[] distances)
        {
            if (distances.Length < raysCount)
            {
                throw new System.ArgumentException("Distances array size should be at least equal to rays count");
            }

            for (var i = 0; i < raysCount; i++)
            {
                distances[i] = this.distances[i];
            }
        }
    }
}