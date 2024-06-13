using UnityEngine;

public abstract class DriverBase : MonoBehaviour
{
    [SerializeField] private float updateInterval = 0.034f;

    [SerializeField] private ObservationsCollector observationsCollector;
    [SerializeField] protected TrackPositionProvider trackPositionProvider;
    [SerializeField] protected LaptimeDataCollector laptimeDataCollector;

    protected Vehicle vehicle;
    protected Racetrack racetrack;
    private float lastUpdateTime = 0;
    private float totalRuntime = 0;

    public delegate void OnDriverUpdatedDelegate(Observations observations, Actions actions);
    public event OnDriverUpdatedDelegate OnDriverUpdated;

    public void Initialize(Vehicle vehicle, Racetrack racetrack)
    {
        this.vehicle = vehicle;
        this.racetrack = racetrack;

        observationsCollector.Initialize(vehicle, racetrack);
        trackPositionProvider.Intialize(racetrack, vehicle.transform);
        laptimeDataCollector.Initialize(trackPositionProvider, vehicle, racetrack);

        InitializeDriver();
        IsInitialized = true;
    }

    private void Update()
    {
        if (!IsInitialized)
        {
            return;
        }

        totalRuntime += Time.deltaTime;
        trackPositionProvider.DoUpdate();
        laptimeDataCollector.DoUpdate();
        var observations = observationsCollector.CollectObservations();

        var currentTime = Time.time;
        if (currentTime - lastUpdateTime >= updateInterval)
        {
            lastUpdateTime = currentTime;

            var actions = UpdateDriver(observations);
            vehicle.SetInputs(actions.Steering, actions.Throttle, actions.Brake);
            OnDriverUpdated?.Invoke(observations, actions);
        }
    }

    protected abstract Actions UpdateDriver(Observations observations);
    protected virtual void InitializeDriver() { }

    public bool IsInitialized { get; private set; } = false;
    public float TotalRuntime => totalRuntime;
}
