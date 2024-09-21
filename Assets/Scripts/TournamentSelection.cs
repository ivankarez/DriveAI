using System;
using System.Collections.Generic;
using System.Linq;

namespace Ivankarez.DriveAI
{
    public class TournamentSelection
    {
        public int TournamentSize { get; }
        public float SurvivalRate { get; }

        public TournamentSelection(int tournamentSize, float survivalRate)
        {
            if (tournamentSize < 2)
            {
                throw new ArgumentException("Tournament size must be at least 2", nameof(tournamentSize));
            }
            if (survivalRate < 0 || survivalRate > 1)
            {
                throw new ArgumentException("Survival rate must be grater than 0 and less than 1", nameof(survivalRate));
            }

            TournamentSize = tournamentSize;
            SurvivalRate = survivalRate;
        }

        public ICollection<Entity> SelectSurvivors(ICollection<Entity> population)
        {
            if (population == null || population.Count < 2)
            {
                throw new ArgumentException("Population must contain at least 2 individuals", nameof(population));
            }

            var remainingPopulation = new HashSet<Entity>(population);
            var survivalCount = (int)(SurvivalRate * population.Count);
            var survivors = new List<Entity>(survivalCount);
            for (var i = 0; i < survivalCount; i++)
            {
                survivors.Add(RunTournament(remainingPopulation));
            }

            return survivors;
        }

        private Entity RunTournament(HashSet<Entity> remainingPopulation)
        {
            var selectedIndividual = remainingPopulation.OrderBy(i => UnityEngine.Random.Range(0f, 1f))
                .Take(TournamentSize)
                .OrderByDescending(i => i.Fitness)
                .First();
            remainingPopulation.Remove(selectedIndividual);

            return selectedIndividual;
        }
    }
}
