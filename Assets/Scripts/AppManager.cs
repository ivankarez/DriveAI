using System;
using UnityEngine;
using UnityEngine.Events;

public class AppManager : MonoBehaviour
{
    public static AppManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        Events.OnTrainingStarted.Invoke();
    }

    public AppManagerEvents Events;
}

[Serializable]
public class AppManagerEvents
{
    public UnityEvent OnTrainingStarted;
    public UnityEvent OnGenerationFinished;
    public UnityEvent OnEpisodeEnded;
    public UnityEvent OnEpisodeStarted;
}
