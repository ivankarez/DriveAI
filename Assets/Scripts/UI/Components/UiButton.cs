using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UiButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public UnityEvent OnClick { get; private set; } = new UnityEvent();

    [SerializeField] private Image background;
    [SerializeField] private float hoverColorMultiplier = 0.9f;
    [SerializeField] private float pressedColorMultiplier = 1.5f;

    private Color normalColor;
    private Color hoverBackgroundColor;
    private Color pressedColor;

    private void Awake()
    {
        normalColor = background.color;
        hoverBackgroundColor = ColorUtils.Multiply(normalColor, hoverColorMultiplier);
        pressedColor = ColorUtils.Multiply(normalColor, pressedColorMultiplier);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        LeanTween.color(background.rectTransform, pressedColor, 0.05f)
            .setLoopPingPong(1)
            .setEaseOutCubic();
        OnClick.Invoke();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        LeanTween.color(background.rectTransform, hoverBackgroundColor, 0.2f)
            .setEaseInOutExpo();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        LeanTween.color(background.rectTransform, normalColor, 0.2f)
            .setEaseInOutExpo();
    }
}
