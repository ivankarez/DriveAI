using UnityEngine;

namespace Ivankarez.DriveAI
{
    public class StaticThirdPersonFollow : MonoBehaviour
    {
        public Transform target;
        public Vector3 followOffset = new(0f, 2f, -5f);
        public Vector3 lookOffset = new(0f, 1f, 0f);
        public float followSpeed = 5f;

        private void Start()
        {
            if (target != null)
            {
                SetTarget(target);
            }
        }

        void FixedUpdate()
        {
            if (target == null)
            {
                return;
            }

            var desiredPosition = target.position - target.forward * followOffset.z + target.up * followOffset.y;
            transform.position = Vector3.Lerp(transform.position, desiredPosition, Time.fixedDeltaTime * followSpeed);
            transform.LookAt(target.position + lookOffset);
        }

        public void SetTarget(Transform target)
        {
            if (target == this.target)
            {
                return;
            }

            if (target == null)
            {
                this.target = null;
                return;
            }

            transform.position = target.position - target.forward * followOffset.z + target.up * followOffset.y;
            transform.LookAt(target.position + lookOffset);
            this.target = target;
        }
    }
}