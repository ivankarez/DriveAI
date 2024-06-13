using UnityEngine;

public class RecordReferenceLearningState : LearningState
{
    [SerializeField] private VehicleSpawner vehicleSpawner;
    [SerializeField] private Racetrack racetrack;
    [SerializeField] private RacingLineVehicleDriver referenceDriverPrefab;
    [SerializeField] private StaticThirdPersonFollow followCamera;
    [SerializeField] private ImitationLearningState trainingState;
    [SerializeField] private ReferenceRun referenceRun;

    private Vehicle vehicle;
    private RacingLineVehicleDriver referenceDriver;

    public override void OnStateEnter()
    {
        if (referenceRun.TryLoad())
        {
            ChangeState(trainingState);
            return;
        }

        vehicle = vehicleSpawner.SpawnVehicle();
        followCamera.target = vehicle.transform;
        referenceDriver = Instantiate(referenceDriverPrefab);
        referenceDriver.Initialize(vehicle, racetrack);
        referenceDriver.OnDriverUpdated += OnDriverUpdated;
        referenceDriver.GetComponent<LaptimeDataCollector>().LapCount.OnChanged += OnLapCompleted;
        referenceRun.StartNewRun();
    }

    public void OnDriverUpdated(Observations observations, Actions actions)
    {
        var sample = new ReferenceRunSample(observations, actions);
        referenceRun.AddSample(sample);
    }

    private void OnLapCompleted(int oldValue, int newValue)
    {
        if (newValue == 1)
        {
            referenceRun.Save();
            ChangeState(trainingState);
        }
    }

    public override void OnStateExit()
    {
        if (vehicle != null)
            Destroy(vehicle.gameObject);
        if (referenceDriver != null)
            Destroy(referenceDriver.gameObject);
    }
}
