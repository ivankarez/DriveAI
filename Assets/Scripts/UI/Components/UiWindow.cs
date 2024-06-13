using UnityEngine;

public class UiWindow : MonoBehaviour
{
    public bool IsOpen { get; private set; }

    [SerializeField] private GameObject rootGameObject;
    [SerializeField] private CanvasGroup canvasGroup;

    private void Awake()
    {
        if (rootGameObject == null)
        {
            rootGameObject = gameObject;
        }

        IsOpen = false;
        rootGameObject.SetActive(false);
    }

    public void Open()
    {
        IsOpen = true;
        rootGameObject.SetActive(true);
        LeanTween.alphaCanvas(canvasGroup, 1f, 0.2f)
            .setEaseInOutExpo();
    }

    public void Close()
    {
        IsOpen = false;
        LeanTween.alphaCanvas(canvasGroup, 1f, 0.2f)
            .setEaseInOutExpo()
            .setOnComplete(() => rootGameObject.SetActive(false));
    }
}
