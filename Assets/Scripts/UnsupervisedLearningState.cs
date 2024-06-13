using System.Collections.Generic;
using UnityEngine;

public class UnsupervisedLearningState : LearningState
{
    [SerializeField] private VehicleSpawner vehicleSpawner;
    [SerializeField] private AiDriver aiDriver;
    [SerializeField] private Racetrack racetrack;
    [SerializeField] private StaticThirdPersonFollow followCamera;

    [Header("Spawn settings")]
    [SerializeField] private float spawnInterval = 1f;
    [SerializeField] private int maxVehicles = 10;

    private readonly List<AiDriver> agents = new();
    private readonly Queue<Individual> testingQueue = new();
    private float lastSpawnTime = 0f;
    private GeneticAlgorithm geneticAlgorithm;

    public void Initialize(ICollection<Individual> individuals)
    {
        geneticAlgorithm = new GeneticAlgorithm(individuals);
    }

    public override void OnStateEnter()
    {
        FillQueue();
    }

    public override void OnStateUpdate()
    {
        if (testingQueue.Count > 0 && agents.Count < maxVehicles && Time.time - lastSpawnTime > spawnInterval)
        {
            SpawnAgent();
        }

        if (testingQueue.Count == 0 && agents.Count == 0)
        {
            Debug.Log("End of generation");
            geneticAlgorithm.StepGeneration();
            FillQueue();
        }
    }

    private void SpawnAgent()
    {
        var individual = testingQueue.Dequeue();
        var vehicle = vehicleSpawner.SpawnVehicle();
        followCamera.target = vehicle.transform;
        var driver = Instantiate(aiDriver);
        driver.Individual = individual;
        driver.Initialize(vehicle, racetrack);
        agents.Add(driver);
        lastSpawnTime = Time.time;

        driver.IsFinished.OnChanged += (_, isFinished) => {
            agents.Remove(driver);
            Destroy(vehicle.gameObject);
            Destroy(driver.gameObject);
        };
    }

    private void FillQueue()
    {
        foreach (var individual in geneticAlgorithm.Population)
        {
            if (!individual.HasFitness)
            {
                testingQueue.Enqueue(individual);
            }
        }
    }
}
