using System.Globalization;
using UnityEngine;

namespace Ivankarez.DriveAI
{
    public class AppDefaults : MonoBehaviour
    {
        private void Awake()
        {
            Application.targetFrameRate = 60;
            QualitySettings.vSyncCount = 0;
            CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
        }
    }
}
