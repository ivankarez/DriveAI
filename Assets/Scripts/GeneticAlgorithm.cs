using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GeneticAlgorithm
{
    const float SURVIVAL_RATE = 0.5f;
    const int TOURNAMENT_SIZE = 5;
    const float MUTATION_RATE = 0.01f;
    const float MUTATION_DEVIATION = 0.1f;

    public List<Individual> Population { get; } = new List<Individual>();
    public long CurrentGeneration { get; private set; } = 0;
    public bool OptForMaximum { get; set; }

    public GeneticAlgorithm(ICollection<Individual> initialPopulation, bool optForMaximum = true)
    {
        Population.AddRange(initialPopulation);
        OptForMaximum = optForMaximum;
    }

    public void StepGeneration()
    {
        if (!Population.All(i => i.HasFitness))
        {
            throw new System.InvalidOperationException("Cannot step generation without fitness for all individuals");
        }

        var populationSize = Population.Count;
        var survivalCount = (int)(SURVIVAL_RATE * populationSize);
        var oldPopulation = Population.OrderByDescending(i => OptForMaximum ? i.Fitness : -i.Fitness).ToList();
        Population.Clear();
        CurrentGeneration++;

        while (Population.Count < populationSize)
        {
            var parent1 = TournamentSelection(oldPopulation);
            var parent2 = TournamentSelection(oldPopulation);
            var child = Crossover(parent1, parent2);
            Population.Add(child);
        }
    }

    private Individual TournamentSelection(List<Individual> oldPopulation)
    {
        return oldPopulation.OrderBy(i => Random.Range(0f, 1f))
            .Take(TOURNAMENT_SIZE)
            .OrderByDescending(i => OptForMaximum ? i.Fitness : -i.Fitness)
            .First();
    }

    private Individual Crossover(Individual parent1, Individual parent2)
    {
        var dnaSize = parent1.Dna.Length;
        var crossoverPoint1 = Random.Range(0, dnaSize);
        var crossoverPoint2 = Random.Range(crossoverPoint1, dnaSize);
        var childDna = new float[dnaSize];

        for (var i = 0; i < dnaSize; i++)
        {
            var parent = (i < crossoverPoint1 || i >= crossoverPoint2) ? parent1 : parent2;
            childDna[i] = parent.Dna[i];
        }
        Mutate(childDna);

        return new Individual(CurrentGeneration, childDna);
    }

    private void Mutate(float[] dna)
    {
        for (var i = 0; i < dna.Length; i++)
        {
            if (Random.Range(0f, 1f) < MUTATION_RATE)
            {
                dna[i] += NormalRandom(0f, MUTATION_DEVIATION);
            }
        }
    }

    private static float NormalRandom(float mean, float stdDev)
    {
        var u1 = 1.0f - Random.Range(0f, 1f);
        var u2 = 1.0f - Random.Range(0f, 1f);
        var randStdNormal = Mathf.Sqrt(-2.0f * Mathf.Log(u1)) * Mathf.Sin(2.0f * Mathf.PI * u2);
        return mean + stdDev * randStdNormal;
    }
}