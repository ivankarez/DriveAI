using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class PretrainWindow : UiWindow
{
    [SerializeField] private BarChart fitnessHistoryChart;
    [SerializeField]
    private TMP_Text generationText,
        performanceText,
        validationErrorText,
        minEvalErrorText,
        avgEvalErrorText,
        timeText;
    [SerializeField] private UiButton stopButton;

    public void ReportFitness(float fitness)
    {
        fitnessHistoryChart.AddValue(fitness);
    }

    public void SetGeneration(long generation)
    {
        generationText.text = FormatUtils.NumberToString(generation);
    }

    public void SetPerformance(float performance)
    {
        performanceText.text = $"{FormatUtils.NumberToString(performance)} it/s";
    }

    public void SetValidationError(float error)
    {
        validationErrorText.text = FormatUtils.NumberToString(error);
    }

    public void SetMinEvalError(float error)
    {
        minEvalErrorText.text = FormatUtils.NumberToString(error);
    }

    public void SetAvgEvalError(float error)
    {
        avgEvalErrorText.text = FormatUtils.NumberToString(error);
    }

    public void SetTime(float time)
    {
        timeText.text = FormatUtils.SecondsToString(time);
    }

    public void SetStopButtonAction(UnityAction action)
    {
        stopButton.OnClick.RemoveAllListeners();
        stopButton.OnClick.AddListener(action);
    }
}
