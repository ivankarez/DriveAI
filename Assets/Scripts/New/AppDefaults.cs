using System.Globalization;
using UnityEngine;

public class AppDefaults : MonoBehaviour
{
    private void Awake()
    {
        Application.targetFrameRate = 60;
        QualitySettings.vSyncCount = 0;
        CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
    }
}
