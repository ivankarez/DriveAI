public class AiDriver : DriverBase
{
    private readonly float[] input = new float[32];
    private readonly MlModel model = new();
    private Individual individual;
    private int waypointsReached = 0;

    protected override void InitializeDriver()
    {
        laptimeDataCollector.IsDnf.OnChanged += OnDriverDnf;
        trackPositionProvider.CurrentTrackPosition.OnChanged += (o, n) => waypointsReached++;
    }

    private void OnDriverDnf(bool oldValue, bool newValue)
    {
        if (newValue)
        {
            ReportFinish();
        }
    }

    protected override Actions UpdateDriver(Observations observations)
    {
        if (IsFinished.Value)
        {
            return new Actions(0f, 0f, 0f);
        }

        observations.ToArray(input);
        var result = model.Feedforward(input);

        if (TotalRuntime > 5f && vehicle.speed < 3f)
        {
            ReportFinish();
        }

        if (TotalRuntime > 5 * 60f)
        {
            ReportFinish();
        }

        return new Actions(result[0], result[1], result[2]);
    }

    private void ReportFinish()
    {
        individual.SetFitness(waypointsReached);
        IsFinished.Value = true;
    }

    public Individual Individual
    {
        get => individual;
        set
        {
            individual = value;
            model.SetParameters(individual.Dna);
        }
    }

    public Signal<bool> IsFinished { get; } = new(false);
}