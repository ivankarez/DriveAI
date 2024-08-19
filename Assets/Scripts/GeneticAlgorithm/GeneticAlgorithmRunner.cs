using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class GeneticAlgorithmRunner : MonoBehaviour
{
    public bool IsRunning { get; private set; }
    public GeneticAlgorithm GeneticAlgorithm { get; private set; }
    public GeneticAlgoritmStats Stats { get; private set; }

    [SerializeField] private int paralellEpisodes = 5;
    [SerializeField] private float episodeStartInterval = 5f;

    private readonly Queue<Individual> waitingIndividuals = new Queue<Individual>();
    private readonly IDictionary<long, Individual> runningIndividuals = new Dictionary<long, Individual>();
    private float lastEpisodeStartTime = 0f;

    public void Run(GeneticAlgorithm geneticAlgorithm, GeneticAlgoritmStats stats)
    {
        if (IsRunning)
        {
            throw new System.InvalidOperationException("Cannot run genetic algorithm runner while it is already running");
        }

        GeneticAlgorithm = geneticAlgorithm ?? throw new System.ArgumentNullException(nameof(geneticAlgorithm));
        Stats = stats ?? throw new System.ArgumentNullException(nameof(stats));
        IsRunning = true;

        FillWaitingQueue();
    }

    public void Stop()
    {
        if (!IsRunning)
        {
            throw new System.InvalidOperationException("Cannot stop genetic algorithm runner while it is not running");
        }

        IsRunning = false;
        GeneticAlgorithm = null;
        Stats = null;
        waitingIndividuals.Clear();
        runningIndividuals.Clear();
        lastEpisodeStartTime = 0f;
    }

    public void EndEpisode(long individualId, float fitness)
    {
        if (!runningIndividuals.TryGetValue(individualId, out var individual))
        {
            throw new System.ArgumentException("Individual with the given ID is not running");
        }

        individual.SetFitness(fitness);
        Stats.OnEpisodeEnded(fitness);
        runningIndividuals.Remove(individualId);
    }

    protected abstract void StartEpisode(Individual individual);

    protected virtual void CreateNextGeneration()
    {
        Stats.OnGenerationEnded();
        GeneticAlgorithm.StepGeneration();
    }

    private void FillWaitingQueue()
    {
        if (!IsRunning)
        {
            throw new System.InvalidOperationException("Cannot fill waiting queue while genetic algorithm runner is not running");
        }

        waitingIndividuals.Clear();

        foreach (var individual in GeneticAlgorithm.Population.Where(i => !i.HasFitness))
        {
            waitingIndividuals.Enqueue(individual);
        }
    }

    private void Update()
    {
        if (!IsRunning)
        {
            return;
        }

        Stats.UpdateRuntime(Time.deltaTime);
        if (waitingIndividuals.Count == 0 && runningIndividuals.Count == 0)
        {
            // Next generation
            CreateNextGeneration();
            FillWaitingQueue();
            return;
        }

        while (runningIndividuals.Count < paralellEpisodes &&
            waitingIndividuals.Count > 0 &&
            Time.realtimeSinceStartup - lastEpisodeStartTime > episodeStartInterval)
        {
            var individual = waitingIndividuals.Dequeue();
            StartEpisode(individual);
            runningIndividuals.Add(individual.Id, individual);
            lastEpisodeStartTime = Time.realtimeSinceStartup;
        }
    }
}
