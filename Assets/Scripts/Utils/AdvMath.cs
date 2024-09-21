using UnityEngine;

public static class AdvMath
{
    public static float MeanSquaredError(float a, float b)
    {
        return (a - b) * (a - b);
    }

    public static float ChangePercentage(float from, float to)
    {
        return (to - from) / from;
    }

    public static float ChangeAmount(float from, float to)
    {
        return to - from;
    }

    public static float DistanceFromPointToLine2D(Vector2 lineStart, Vector2 lineEnd, Vector2 position)
    {
        float a = lineEnd.y - lineStart.y;
        float b = -(lineEnd.x - lineStart.x);
        float c = lineEnd.x * lineStart.y - lineEnd.y * lineStart.x;

        float distance = (a * position.x + b * position.y + c)
            / Mathf.Sqrt(a * a + b * b);
        return distance;
    }
}
