using System;
using UnityEngine;

public class LearnSceneManager : MonoBehaviour
{
    [SerializeField] private LearningState startState;

    private LearningState currentState;

    private void Start()
    {
        ChangeState(startState);
    }

    private void ChangeState(LearningState newState)
    {
        newState.ChangeState = ChangeState;
        if (currentState != null)
        {
            currentState.OnStateExit();
        }
        currentState = newState;
        currentState.OnStateEnter();
    }

    private void Update()
    {
        if (currentState != null)
        {
            currentState.OnStateUpdate();
        }
    }
}

public abstract class LearningState : MonoBehaviour
{
    public Action<LearningState> ChangeState { get; set; }

    public virtual void OnStateEnter() { }
    public virtual void OnStateExit() { }
    public virtual void OnStateUpdate() { }
}
