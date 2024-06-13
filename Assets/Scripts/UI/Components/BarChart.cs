using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BarChart : MonoBehaviour
{
    [SerializeField] private RectTransform barContainer;
    [SerializeField] private RectTransform barPrefab;

    [SerializeField] private float spacing = 8f;
    [SerializeField] private float padding = 8f;
    [SerializeField] private float topPadding = 16f;
    [SerializeField] private int valueCount = 30;

    private readonly List<RectTransform> bars = new();
    private readonly List<float> values = new();
    private float maxValue = 0f;
    private float barWidth;


    private void Awake()
    {
        barPrefab.gameObject.SetActive(false);
        barWidth = (barContainer.rect.width - (padding * 2) - (spacing * (valueCount - 1))) / valueCount;

        for (int i = 0; i < valueCount; i++)
        {
            RectTransform bar = Instantiate(barPrefab, barContainer);
            bar.gameObject.SetActive(true);
            bar.sizeDelta = new Vector2(barWidth, 0);
            bar.anchoredPosition = new Vector2(padding + (i * (barWidth + spacing)), 0);
            bars.Add(bar);
        }
    }

    public void AddValue(float value)
    {
        values.Add(value);

        if (values.Count > valueCount)
        {
            values.RemoveAt(0);
        }
        maxValue = values.Max();

        var scale = maxValue == 0 ? 0 : (barContainer.rect.height - topPadding) / maxValue;
        for (int i = 0; i < values.Count; i++)
        {
            var bar = bars[i];
            bar.sizeDelta = new Vector2(barWidth, values[i] * scale);
        }
    }
}
