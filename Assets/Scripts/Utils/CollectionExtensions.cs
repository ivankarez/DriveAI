using System.Collections.Generic;

public static class CollectionExtensions
{
    public static T CircularIndex<T>(this T[] array, int index)
    {
        return array[array.CalculateCircularIndex(index)];
    }


    public static int CalculateCircularIndex<T>(this T[] array, int index)
    {
        return (index % array.Length + array.Length) % array.Length;
    }

    public static T SelectRandom<T>(this List<T> list)
    {
        return list[UnityEngine.Random.Range(0, list.Count)];
    }
}
