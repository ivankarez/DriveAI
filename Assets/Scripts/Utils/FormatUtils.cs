using System.Globalization;

public static class FormatUtils
{
    public static string SecondsToString(float seconds)
    {
        if (seconds < 60)
        {
            return $"00:{seconds.ToString("00.000", CultureInfo.InvariantCulture)}";
        }

        if (seconds < 3600)
        {
            return $"{(int)seconds / 60:00}:{(seconds % 60).ToString("00.000", CultureInfo.InvariantCulture)}";
        }

        return $"{(int)seconds / 3600:00}:{(int)seconds % 3600 / 60:00}:{(seconds % 60).ToString("00.000", CultureInfo.InvariantCulture)}";
    }

    public static string NumberToString(float number)
    {
        if (number < 0.01f)
        {
            return number.ToString("f4", CultureInfo.InvariantCulture);
        }

        if (number < 1f)
        {
            return number.ToString("f2", CultureInfo.InvariantCulture);
        }

        if (number < 1_000f)
        {
            return number.ToString("f0", CultureInfo.InvariantCulture);
        }

        if (number < 100_000f)
        {
            return $"{(number / 1000f).ToString("f2", CultureInfo.InvariantCulture)}K";
        }

        return $"{(number / 1000f).ToString("f0", CultureInfo.InvariantCulture)}K";
    }
}
