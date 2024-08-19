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
}
