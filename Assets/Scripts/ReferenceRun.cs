using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class ReferenceRun : MonoBehaviour
{
    private readonly List<ReferenceRunSample> samples = new();

    public void Save()
    {
        var path = Path.Combine(Application.persistentDataPath, "reference_run.json");
        File.WriteAllText(path, JsonUtility.ToJson(new ReferenceRunData(samples)));
        Debug.Log($"Reference run saved to {path}");
    }

    public bool TryLoad()
    {
        var path = Path.Combine(Application.persistentDataPath, "reference_run.json");
        if (!File.Exists(path))
        {
            return false;
        }
        samples.Clear();
        samples.AddRange(JsonUtility.FromJson<ReferenceRunData>(File.ReadAllText(path)).Samples);
        Debug.Log($"Reference run loaded from {path}");

        return true;
    }

    public void StartNewRun()
    {
        samples.Clear();
    }

    public void AddSample(ReferenceRunSample sample)
    {
        samples.Add(sample);
    }   

    public IReadOnlyList<ReferenceRunSample> Samples => samples;
}

[System.Serializable]
public class ReferenceRunSample
{
    [SerializeField] private Observations observations;
    [SerializeField] private Actions actions;

    public Observations Observations => observations;
    public Actions Actions => actions;

    public ReferenceRunSample(Observations observations, Actions actions)
    {
        this.observations = observations;
        this.actions = actions;
    }
}

[System.Serializable]
public class ReferenceRunData
{
    [SerializeField] private List<ReferenceRunSample> samples;

    public ReferenceRunData(List<ReferenceRunSample> samples)
    {
        this.samples = samples;
    }

    public List<ReferenceRunSample> Samples => samples;
}