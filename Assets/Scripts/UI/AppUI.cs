using UnityEngine;

public class AppUI : MonoBehaviour
{
    public static T OpenWindow<T>(T windowPrefab) where T : UiWindow
    {
        T window = Instantiate(windowPrefab);
        window.Open();

        return window;
    }
}
