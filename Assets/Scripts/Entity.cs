using System.Collections.Generic;
using System.IO;

namespace Ivankarez.DriveAI
{
    public class Entity
    {
        private float fitness;
        private readonly float[] dna;

        public string Id { get; }
        public ulong Generation { get; }
        public ulong GenerationId { get; }

        public bool HasFitness { get; private set; }
        public IReadOnlyList<float> Dna => dna;
        public float Fitness
        {
            get
            {
                if (!HasFitness)
                {
                    throw new System.Exception($"Fitness was not set on {nameof(Entity)} #{Id}");
                }

                return fitness;
            }

            set
            {
                if (HasFitness)
                {
                    throw new System.Exception($"Fitness was already set on {nameof(Entity)} #{Id}");
                }

                HasFitness = true;
                fitness = value;
            }
        }

        public Entity(ulong generation, ulong generationId, float[] dna)
        {
            if (dna == null)
            {
                throw new System.ArgumentNullException(nameof(dna));
            }
            if (dna.Length == 0)
            {
                throw new System.ArgumentException("DNA cannot be empty", nameof(dna));
            }

            Id = $"{generation}-{generationId}";
            Generation = generation;
            GenerationId = generationId;
            this.dna = dna;
            HasFitness = false;
        }

        public void Serialize(BinaryWriter writer)
        {
            writer.Write(Generation);
            writer.Write(GenerationId);
            writer.Write(HasFitness);
            writer.Write(fitness);
            writer.Write(dna.Length);
            foreach (var gene in dna)
            {
                writer.Write(gene);
            }
        }

        public static Entity Deserialize(BinaryReader reader)
        {
            var generation = reader.ReadUInt64();
            var generationId = reader.ReadUInt64();
            var hasFitness = reader.ReadBoolean();
            var fitness = reader.ReadSingle();
            var dnaLength = reader.ReadInt32();
            var dna = new float[dnaLength];
            for (int i = 0; i < dnaLength; i++)
            {
                dna[i] = reader.ReadSingle();
            }

            var entity = new Entity(generation, generationId, dna)
            {
                fitness = fitness,
                HasFitness = hasFitness
            };
            return entity;
        }
    }
}
