using UnityEngine;


public class ColorUtils
{
    public static Color Multiply(Color color, float multiplier)
    {
        return new Color(color.r * multiplier, color.g * multiplier, color.b * multiplier, color.a);
    }
}
