using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Ivankarez.DriveAI
{
    public class GeneticAlgorithm : MonoBehaviour
    {
        [SerializeField] private int populationSize = 10;
        [SerializeField] private int paralelEpisodes = 3;
        [SerializeField] private Agent agentPrefab = null;
        [SerializeField] private string runName = "";

        private readonly TournamentSelection selection = new(3, 0.5f);
        private readonly Reproduction reproduction = new(0.03f, 0.05f);
        private readonly List<Entity> population = new();
        private readonly Queue<Entity> episodeQueue = new();
        private readonly List<Agent> agents = new();
        private ulong generation = 0;
        private float runtime = 0f;
        private Entity bestEntity;

        private void Start()
        {
            if (runName == "")
            {
                enabled = false;
                throw new InvalidOperationException("Run name is not set");
            }

            var rootFolder = GetStoragePath("/");
            Debug.Log($"Storage path: {rootFolder}");
            if (Directory.Exists(rootFolder))
            {
                Load();
            }
            else
            {
                Directory.CreateDirectory(rootFolder);
            }

            if (population.Count == 0)
            {
                for (int i = 0; i < populationSize; i++)
                {
                    var entity = new Entity(generation, (ulong)i, DnaUtils.CreateNewDna());
                    population.Add(entity);
                    episodeQueue.Enqueue(entity);
                }
            }
            else
            {
                CreateNextGeneration();
            }
        }

        private void Update()
        {
            if (agents.Count < paralelEpisodes)
            {
                if (episodeQueue.Count > 0)
                {
                    var entity = episodeQueue.Dequeue();
                    var agent = Instantiate(agentPrefab);
                    agent.Initialize(entity, OnEpisodeEnd);
                    agents.Add(agent);
                }
                else if (agents.Count == 0)
                {
                    var genBest = population.OrderByDescending(e => e.Fitness).First();
                    var newBest = false;
                    if (bestEntity == null || genBest.Fitness > bestEntity.Fitness)
                    {
                        bestEntity = genBest;
                        newBest = true;
                    }

                    var bestFitness = genBest.Fitness;
                    var averageFitness = population.Average(e => e.Fitness);
                    int runtimeHours = (int)(runtime / 60 / 60);
                    int runtimeMinutes = (int)(runtime / 60) % 60;
                    Debug.Log($"[SUMMARY] Generation:{generation}, Best:{bestFitness:f2}{(newBest ? " (AT)" : "")}, Average:{averageFitness:f2}, Runtime:{runtimeHours:d2}:{runtimeMinutes:d2}");
                    Save();
                    CreateNextGeneration();
                }
            }

            runtime += Time.deltaTime;
        }

        private void CreateNextGeneration()
        {
            generation++;
            var populationSize = population.Count;
            var survivors = selection.SelectSurvivors(population);
            var newPopulationDnas = reproduction.CombineIndividuals(survivors, populationSize);
            population.Clear();
            foreach (var dna in newPopulationDnas)
            {
                var entity = new Entity(generation, (ulong)population.Count, dna);
                population.Add(entity);
                episodeQueue.Enqueue(entity);
            }
        }

        private void OnEpisodeEnd(Agent agent)
        {
            agents.Remove(agent);
            Destroy(agent.gameObject);
        }

        private void Save()
        {
            // Write performance CSV
            var csvFile = GetStoragePath("fitness.csv");
            using var writer = new StreamWriter(csvFile, true);
            writer.WriteLine($"{generation},{runtime},{population.Max(i => i.Fitness)},{population.Average(i => i.Fitness)}");

            // Write population
            var rootFolder = GetStoragePath("population.bin");
            using var binaryWriter = new BinaryWriter(File.Open(rootFolder, FileMode.Create));
            binaryWriter.Write(population.Count);
            foreach (var entity in population)
            {
                entity.Serialize(binaryWriter);
            }

            // Write genetic algortihm state
            var stateFile = GetStoragePath("state.bin");
            using var stateWriter = new BinaryWriter(File.Open(stateFile, FileMode.Create));
            stateWriter.Write(generation);
            stateWriter.Write(runtime);
            bestEntity.Serialize(stateWriter);
        }

        private void Load()
        {
            // Read population
            var populationFile = GetStoragePath("population.bin");
            using var binaryReader = new BinaryReader(File.Open(populationFile, FileMode.Open));
            var populationSize = binaryReader.ReadInt32();
            for (int i = 0; i < populationSize; i++)
            {
                var entity = Entity.Deserialize(binaryReader);
                population.Add(entity);
            }

            // Read genetic algortihm state
            var stateFile = GetStoragePath("state.bin");
            using var stateReader = new BinaryReader(File.Open(stateFile, FileMode.Open));
            generation = stateReader.ReadUInt64();
            runtime = stateReader.ReadSingle();
            bestEntity = Entity.Deserialize(stateReader);
        }

        private string GetStoragePath(string path)
        {
            return Path.Join(Application.persistentDataPath, runName, path);
        }
    }
}
