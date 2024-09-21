using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Ivankarez.DriveAI
{
    public class Reproduction
    {
        public float MutationRate { get; }
        public float MutationDeviation { get; }

        public Reproduction(float mutationRate, float mutationDeviation)
        {
            if (mutationRate < 0 || mutationRate > 1)
            {
                throw new ArgumentOutOfRangeException(nameof(mutationRate), "Mutation rate must be between 0 and 1");
            }
            if (mutationDeviation < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(mutationDeviation), "Mutation deviation must be greater than 0");
            }

            MutationRate = mutationRate;
            MutationDeviation = mutationDeviation;
        }

        public ICollection<float[]> CombineIndividuals(ICollection<Entity> individuals, int populationSize)
        {
            if (individuals == null || individuals.Count < 2)
            {
                throw new ArgumentException("Cannot combine individuals with less than 2 elements", nameof(individuals));
            }
            if (populationSize <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(populationSize), "Population size must be greater than 0");
            }

            var population = new List<float[]>(populationSize);

            do
            {
                var parents = individuals.OrderBy(i => Random.value)
                    .Take(2);
                var parent1 = parents.First();
                var parent2 = parents.Last();
                var child = Crossover(parent1, parent2);
                population.Add(child);
            } while (population.Count < populationSize);

            return population;
        }

        private float[] Crossover(Entity parent1, Entity parent2)
        {
            var dnaSize = parent1.Dna.Count;
            var crossoverPoint1 = Random.Range(0, dnaSize);
            var crossoverPoint2 = Random.Range(crossoverPoint1, dnaSize);
            var childDna = new float[dnaSize];

            for (var i = 0; i < dnaSize; i++)
            {
                var parent = (i < crossoverPoint1 || i >= crossoverPoint2) ? parent1 : parent2;
                childDna[i] = parent.Dna[i];
            }
            Mutate(childDna);

            return childDna;
        }

        private void Mutate(float[] dna)
        {
            for (var i = 0; i < dna.Length; i++)
            {
                if (Random.Range(0f, 1f) < MutationRate)
                {
                    dna[i] += NormalRandom(0f, MutationDeviation);
                    dna[i] = Mathf.Clamp(dna[i], -1f, 1f);
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
}
