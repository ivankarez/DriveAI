using Ivankarez.DriveAI;
using System.IO;
using UnityEngine;

public class RaceMode : MonoBehaviour
{
    [SerializeField] private Racetrack racetrack;
    [SerializeField] private Agent agentPrefab;
    [SerializeField] private string runName;

    private void Start()
    {
        var agent = Instantiate(agentPrefab);
        var bestEntity = LoadBest();
        agent.Initialize(bestEntity, OnEpisodeEnd);
    }

    private void OnEpisodeEnd(Agent agent)
    {
        Debug.Log($"Episode ended with fitness: {agent.Fitness}");
    }

    private Entity LoadBest()
    {
        // Read genetic algortihm state
        var stateFile = GetStoragePath("state.bin");
        using var stateReader = new BinaryReader(File.Open(stateFile, FileMode.Open));
        var generation = stateReader.ReadUInt64();
        var runtime = stateReader.ReadSingle();
        return Entity.Deserialize(stateReader);
    }

    private string GetStoragePath(string path)
    {
        return Path.Join(Application.persistentDataPath, runName, path);
    }
}
