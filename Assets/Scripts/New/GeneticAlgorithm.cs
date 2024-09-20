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

        private readonly TournamentSelection selection = new(3, 0.5f);
        private readonly Reproduction reproduction = new(0.03f, 0.05f);
        private readonly List<Entity> population = new();
        private readonly Queue<Entity> episodeQueue = new();
        private readonly List<Agent> agents = new();
        private ulong generation = 0;

        private void Start()
        {
            var rootFolder = Path.Combine(Application.persistentDataPath, "GeneticAlgortihm");
            if (!Directory.Exists(rootFolder))
            {
                Directory.CreateDirectory(rootFolder);
            }

            Load();

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
                    Debug.Log($"All episodes finished in generation {generation}. Best fitness {population.Max(e => e.Fitness)} / Average fitness: {population.Average(e => e.Fitness):f2}");
                    Save();
                    CreateNextGeneration();
                }
            }
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
            var csvFile = Path.Combine(Application.persistentDataPath, "fitness.txt");
            using var writer = new StreamWriter(csvFile, true);

            var rootFolder = Path.Join(Application.persistentDataPath, "GeneticAlgortihm");
            writer.WriteLine($"{generation};{population.Max(i => i.Fitness)};{population.Average(i => i.Fitness)}");

            foreach (var filePath in Directory.GetFiles(rootFolder))
            {
                File.Delete(filePath);
            }

            foreach (var entity in population)
            {
                var filePath = Path.Join(rootFolder, $"entity_{entity.Id}.bin");
                using var binaryWriter = new BinaryWriter(File.Open(filePath, FileMode.Create));
                entity.Serialize(binaryWriter);
            }
        }

        private void Load()
        {
            var rootFolder = Path.Join(Application.persistentDataPath, "GeneticAlgortihm");
            if (!Directory.Exists(rootFolder))
            {
                return;
            }

            foreach (var filePath in Directory.GetFiles(rootFolder))
            {
                using var binaryReader = new BinaryReader(File.Open(filePath, FileMode.Open));
                var entity = Entity.Deserialize(binaryReader);
                population.Add(entity);
            }
        }
    }
}
