using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class ImitationLearningState : LearningState
{
    [SerializeField] private UnsupervisedLearningState unsupervisedLearningState;
    [SerializeField] private ReferenceRun referenceRun;
    [SerializeField] private PretrainWindow windowPrefab;
    [SerializeField] private long maxGenerations = 1000;
    [SerializeField] private float clusterThreshold = 1f;

    private GeneticAlgorithm geneticAlgorithm;
    private bool stopFlag = false;
    private long generation = 0;
    private Individual bestIndividual;
    private long iterations = 0;
    private float trainingTime = 0f;
    private PretrainWindow window;

    public override void OnStateEnter()
    {
        var samples = referenceRun.Samples;
        var prunedSamples = ProcessSamples(samples);
        Debug.Log($"Pruned samples: {prunedSamples.Count}. Prune rate: {100f * prunedSamples.Count / samples.Count:f0}%");

        // Split samples into training and validation sets randomly
        var trainingSet = new List<ImitationLearningSample>();
        var validationSet = new List<ImitationLearningSample>();
        foreach (var sample in prunedSamples)
        {
            if (Random.value < 0.8f)
            {
                trainingSet.Add(sample);
            }
            else
            {
                validationSet.Add(sample);
            }
        }

        window = AppUI.OpenWindow(windowPrefab);
        window.SetStopButtonAction(() => stopFlag = true);

        iterations = 0;
        trainingTime = 0f;

        StartCoroutine(RunGeneticAlgorithm(trainingSet, validationSet));
    }

    public override void OnStateUpdate()
    {
        window.SetPerformance(iterations / trainingTime);
        window.SetTime(trainingTime);

        trainingTime += Time.deltaTime;
    }

    private IReadOnlyList<ImitationLearningSample> ProcessSamples(IReadOnlyList<ReferenceRunSample> samples)
    {
        var prunedSamples = new List<ImitationLearningSample>(samples.Count)
        {
            ImitationLearningSample.FromReferenceRunSample(samples[0])
        };

        for (int i = 1; i < samples.Count; i++)
        {
            var prevSample = prunedSamples.Last();
            var currentSample = ImitationLearningSample.FromReferenceRunSample(samples[i]);
            if (currentSample.Distance(prevSample) > clusterThreshold)
            {
                prunedSamples.Add(currentSample);
            }
        }

        return prunedSamples;
    }

    private IEnumerator RunGeneticAlgorithm(IReadOnlyList<ImitationLearningSample> trainingSet, IReadOnlyList<ImitationLearningSample> validationSet)
    {
        var model = new MlModel();
        var population = CreateInitialPopulation(100, model.GetParameters().Length);
        geneticAlgorithm = new GeneticAlgorithm(population, optForMaximum: false);

        while (generation < maxGenerations)
        {
            var individualsToEvaluate = geneticAlgorithm.Population.Where(i => !i.HasFitness);
            var sumError = 0f;
            var evaluationCount = 0;
            foreach (var individual in individualsToEvaluate)
            {
                var error = EvaluateIndividual(individual, model, trainingSet);
                individual.SetFitness(error);
                sumError += error;
                evaluationCount++;
                if (bestIndividual == null || individual.Fitness < bestIndividual.Fitness)
                {
                    bestIndividual = individual;
                    window.SetMinEvalError(bestIndividual.Fitness);
                }
                if (stopFlag)
                {
                    break;
                }
                yield return null;

            }
            if (stopFlag)
            {
                break;
            }

            var populationAverageError = sumError / evaluationCount;
            window.SetAvgEvalError(populationAverageError);
            var validationError = EvaluateIndividual(bestIndividual, model, validationSet);
            window.SetValidationError(validationError);
            window.ReportFitness(validationError);

            geneticAlgorithm.StepGeneration();
            generation = geneticAlgorithm.CurrentGeneration;
            window.SetGeneration(generation);
        }

        Debug.Log($"Pretrain ended with generation {generation}. Best fitness: {bestIndividual.Fitness}");
        window.Close();
        
        unsupervisedLearningState.Initialize(geneticAlgorithm.Population);
        ChangeState(unsupervisedLearningState);
    }

    private float EvaluateIndividual(Individual individual, MlModel model, IReadOnlyList<ImitationLearningSample> samples)
    {
        model.SetParameters(individual.Dna);

        var totalError = 0f;
        Parallel.ForEach(samples, sample =>
        {
            var output = model.Feedforward(sample.Input);
            var error = sample.Output.Zip(output, AdvMath.MeanSquaredError).Sum();
            totalError += error;
            iterations++;
        });

        return totalError / samples.Count;
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
}

class ImitationLearningSample
{
    public float[] Input { get; set; }
    public float[] Output { get; set; }

    public float Distance(ImitationLearningSample other)
    {
        float distance = 0f;

        for (int i = 0; i < Input.Length; i++)
        {
            distance += Mathf.Abs(Input[i] - other.Input[i]);
        }

        for (int i = 0; i < Output.Length; i++)
        {
            distance += Mathf.Abs(Output[i] - other.Output[i]);
        }

        return distance;
    }

    public static ImitationLearningSample FromReferenceRunSample(ReferenceRunSample sample)
    {
        return new ImitationLearningSample
        {
            Input = sample.Observations.ToArray(),
            Output = sample.Actions.ToArray(),
        };
    }
}
