using System.Collections.Generic;
using System.Linq;

public class GeneticAlgorithm
{
    private readonly SelectionStrategy selection;
    private readonly ReproductionStrategy reproduction;

    public List<Individual> Population { get; } = new List<Individual>();

    public GeneticAlgorithm(ICollection<Individual> population,
        SelectionStrategy selection,
        ReproductionStrategy reproduction)
    {
        Population.AddRange(population);
        this.selection = selection;
        this.reproduction = reproduction;
    }

    public void StepGeneration() 
    {
        if (!Population.All(i => i.HasFitness))
        {
            throw new System.InvalidOperationException("Cannot step generation without fitness for all individuals");
        }

        var populationSize = Population.Count;
        var survivors = selection.SelectSurvivors(Population);
        var newPopulation = reproduction.CombineIndividuals(survivors, populationSize);

        Population.Clear();
        Population.AddRange(newPopulation);
    }
}