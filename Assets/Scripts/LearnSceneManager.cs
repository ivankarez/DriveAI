using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LearnSceneManager : GeneticAlgorithmRunner
{
    [SerializeField] private VehicleSpawner vehicleSpawner;
    [SerializeField] private AiDriver aiDriverPrefab;
    [SerializeField] private Racetrack racetrack;
    [SerializeField] private StaticThirdPersonFollow followCamera;

    private readonly List<AiDriver> agents = new();

    private void Start()
    {
        var individuals = Enumerable.Range(0, 26)
            .Select(_ => new Individual(0, new MlModel().GetParameters()))
            .ToList();

        var geneticAlgorithm = new GeneticAlgorithm(individuals,
            new SelectionStrategy(5, 0.5f),
            new ReproductionStrategy(0.03f, 0.3f));
        Run(geneticAlgorithm, new GeneticAlgoritmStats(100));
    }

    protected override void CreateNextGeneration()
    {
        var currentGenerationStats = Stats.CurrentGenerationStats;
        Debug.Log($"Generation #{Stats.GenerationCount}. {{ Best fitness: {currentGenerationStats.BestFitness:f2}, Average fitness: {currentGenerationStats.AverageFitness:f2}, Fitness improvement: {Stats.AverageFitnessImprovement:f2} }}");

        base.CreateNextGeneration();
    }

    protected override void StartEpisode(Individual individual)
    {
        SpawnAgent(individual);
    }

    private void SpawnAgent(Individual individual)
    {
        var vehicle = vehicleSpawner.SpawnVehicle();
        //followCamera.target = vehicle.transform;
        var driver = Instantiate(aiDriverPrefab);
        driver.gameObject.name = $"AiDriver (#{individual.Id})";
        driver.Individual = individual;
        driver.Initialize(vehicle, racetrack);
        agents.Add(driver);

        driver.IsFinished.OnChanged += (_, isFinished) => {
            agents.Remove(driver);
            EndEpisode(individual.Id, individual.Fitness);
            Destroy(vehicle.gameObject);
            Destroy(driver.gameObject);
        };
    }
}
