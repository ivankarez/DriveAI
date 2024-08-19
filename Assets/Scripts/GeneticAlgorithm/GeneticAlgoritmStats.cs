using System.Collections.Generic;
using System.Linq;

public class GeneticAlgoritmStats
{
    private readonly int retainedGenerationsCount;
    private readonly List<GenerationStats> generationsStats;

    public long GenerationCount { get; private set; }
    public float BestFitness { get; private set; }
    public float TotalRuntime { get; private set; }
    public long TotalEpisodeCount { get; private set; }
    public float AverageFitnessImprovement { get; private set; }
    public IReadOnlyCollection<GenerationStats> GenerationsStats => generationsStats;
    public GenerationStats CurrentGenerationStats => generationsStats.Last();

    public GeneticAlgoritmStats(int retainedGenerationsCount)
    {
        this.retainedGenerationsCount = retainedGenerationsCount;
        generationsStats = new List<GenerationStats>(retainedGenerationsCount)
        {
            new(0)
        };

        BestFitness = float.MinValue;
    }

    public void OnEpisodeEnded(float fitness)
    {
        TotalEpisodeCount++;
        CurrentGenerationStats.OnEpisodeEnded(fitness);

        if (fitness > BestFitness)
        {
            BestFitness = fitness;
        }
    }

    public void OnGenerationEnded()
    {
        GenerationCount++;
        var newGenerationStats = new GenerationStats(GenerationCount, CurrentGenerationStats);
        generationsStats.Add(newGenerationStats);

        if (generationsStats.Count > retainedGenerationsCount)
        {
            generationsStats.RemoveAt(generationsStats.Count - 1);
        }

        AverageFitnessImprovement = generationsStats.Average(gs => gs.FitnessImprovement);
    }

    public void UpdateRuntime(float deltaTime)
    {
        TotalRuntime += deltaTime;
        CurrentGenerationStats.UpdateRuntime(deltaTime);
    }
}

public class GenerationStats
{
    private readonly GenerationStats previousGenerationStats;

    public long Generation { get; private set; }
    public float BestFitness { get; private set; }
    public float AverageFitness { get; private set; }
    public float WorstFitness { get; private set; }
    public float FitnessImprovement { get; private set; }
    public float Runtime { get; private set; }
    public long EpisodeCount { get; private set; }

    public GenerationStats(long generation, GenerationStats previousGenerationStats = null)
    {
        Generation = generation;
        this.previousGenerationStats = previousGenerationStats;
        BestFitness = float.MinValue;
        WorstFitness = float.MaxValue;
    }

    public void OnEpisodeEnded(float fitness)
    {
        EpisodeCount++;
        AverageFitness = (AverageFitness * (EpisodeCount - 1) + fitness) / EpisodeCount;
        FitnessImprovement = previousGenerationStats == null ? 0 : AdvMath.ChangeAmount(previousGenerationStats.AverageFitness, AverageFitness);

        if (fitness > BestFitness)
        {
            BestFitness = fitness;
        }
        if (fitness < WorstFitness)
        {
            WorstFitness = fitness;
        }
    }

    public void UpdateRuntime(float deltaTime)
    {
        Runtime += deltaTime;
    }
}
