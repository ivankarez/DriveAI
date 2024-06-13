public class Individual
{
    private static long idCounter = 0;

    public long Id { get; }
    public long Generation { get; }
    public float[] Dna { get; }
    public float Fitness { get; private set; }
    public bool HasFitness { get; private set; }

    public Individual(long generation, float[] dna)
    {
        Id = idCounter++;
        Generation = generation;
        Dna = dna;
        Fitness = 0;
        HasFitness = false;
    }

    public void SetFitness(float fitness)
    {
        Fitness = fitness;
        HasFitness = true;
    }
}
