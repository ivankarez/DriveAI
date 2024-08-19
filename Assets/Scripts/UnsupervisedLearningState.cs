using System.Collections.Generic;
using UnityEngine;

public class UnsupervisedLearningState : MonoBehaviour
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

    private void Awake()
    {
        enabled = false;
        AppManager.Instance.Events.OnTrainingStarted.AddListener(StartTraining);
    }

    private void StartTraining()
    {
        Debug.Log("Starting training");
        enabled = true;
        var individuals = CreateInitialPopulation(100, new MlModel().GetParameters().Length);
        
        FillQueue();
    }

    private List<Individual> CreateInitialPopulation(int populationSize, int dnaLength)
    {
        var population = new List<Individual>(populationSize);
        for (int i = 0; i < populationSize; i++)
        {
            var dna = CreateRandomDna(dnaLength);
            var individual = new Individual(0, dna);
            population.Add(individual);
        }

        return population;
    }

    private float[] CreateRandomDna(int dnaLenght)
    {
        var dna = new float[dnaLenght];
        for (int i = 0; i < dnaLenght; i++)
        {
            dna[i] = Random.Range(-1f, 1f);
        }

        return dna;
    }

    public void Update()
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
