using UnityEngine;

namespace Ivankarez.DriveAI
{
    public class Vehicle : MonoBehaviour
    {
        [SerializeField] private Rigidbody vehicleRigidbody;
        [SerializeField] private Transform centerOfMass;
        [SerializeField] private float motorTorque = 1000f;
        [SerializeField] private float brakeTorque = 1000f;
        [SerializeField] private WheelCollider[] wheelColliders = new WheelCollider[4];
        [SerializeField] private Transform[] wheelMeshes = new Transform[4];
        [SerializeField] private MeshRenderer chassisRenderer;
        public Transform nose;
        public float maxSteerAngle = 15f;
        public AnimationCurve downforceBySpeed;
        public float maxSpeed = 340f;
        public Color ChassisColor
        {
            get => chassisMaterial.color;
            set => chassisMaterial.color = value;
        }

        public readonly WheelHit?[] wheelHits = new WheelHit?[4];
        public readonly bool[] isAbsActive = new bool[4];
        private readonly WheelFrictionCurve[] baseWheelFwdFrictionCurves = new WheelFrictionCurve[4];
        private readonly WheelFrictionCurve[] baseWheelSideFrictionCurves = new WheelFrictionCurve[4];
        [HideInInspector] public float speed = 0f;
        [HideInInspector] public float downforce = 0f;

        private Material chassisMaterial;
        private float steering = 0f;
        private float throttle = 0f;
        private float brake = 0f;

        private void Awake()
        {
            chassisMaterial = chassisRenderer.materials[0];
        }

        private void Start()
        {
            for (int i = 0; i < wheelColliders.Length; i++)
            {
                var wheelCollider = wheelColliders[i];
                wheelCollider.ConfigureVehicleSubsteps(5.0f, 30, 10);
                baseWheelFwdFrictionCurves[i] = wheelCollider.forwardFriction;
                baseWheelSideFrictionCurves[i] = wheelCollider.sidewaysFriction;
            }

            vehicleRigidbody.centerOfMass = centerOfMass.localPosition;
        }

        private void Update()
        {
            Move();
            UpdateWheelMeshes();
            speed = vehicleRigidbody.velocity.magnitude * 3.6f;
        }

        private void UpdateWheelMeshes()
        {
            for (int i = 0; i < wheelColliders.Length; i++)
            {
                wheelColliders[i].GetWorldPose(out Vector3 position, out Quaternion rotation);
                wheelMeshes[i].position = position;
                wheelMeshes[i].rotation = rotation;
            }
        }

        public void SetInputs(float steering, float throttle, float brake)
        {
            this.steering = steering;
            this.throttle = throttle;
            this.brake = brake;
        }

        private void Move()
        {
            downforce = downforceBySpeed.Evaluate(speed);
            vehicleRigidbody.AddForce(downforce * -transform.up);
            var downforceMultiplier = 1 + (downforce / vehicleRigidbody.mass);

            var roll = transform.InverseTransformDirection(vehicleRigidbody.angularVelocity).z;
            var rollMultiplier = roll / 45f;

            for (int i = 0; i < 4; i++)
            {
                var rollFriction = i % 2 == 0 ? rollMultiplier : -rollMultiplier;
                var friction = rollFriction + downforceMultiplier;

                wheelColliders[i].forwardFriction = AdjustFriction(baseWheelFwdFrictionCurves[i], friction);
                wheelColliders[i].sidewaysFriction = AdjustFriction(baseWheelSideFrictionCurves[i], friction);
            }

            for (int i = 0; i < 2; i++)
            {
                wheelColliders[i].steerAngle = maxSteerAngle * steering;
            }

            for (int i = 2; i < 4; i++)
            {
                wheelColliders[i].motorTorque = motorTorque * throttle;
            }

            for (int i = 0; i < 4; i++)
            {
                var brakeMultiplier = i < 2 ? .6f : .4f;
                if (wheelColliders[i].GetGroundHit(out WheelHit hit))
                {
                    wheelHits[i] = hit;
                    isAbsActive[i] = -hit.forwardSlip > 0.9f;
                    if (isAbsActive[i])
                    {
                        brakeMultiplier = 0f;
                    }
                }
                else
                {
                    wheelHits[i] = null;
                }
                wheelColliders[i].brakeTorque = brakeTorque * brake * brakeMultiplier;
            }
        }

        private WheelFrictionCurve AdjustFriction(WheelFrictionCurve baseCurve, float frictionMultiplier)
        {
            baseCurve.stiffness *= frictionMultiplier;
            return baseCurve;
        }
    }
}
