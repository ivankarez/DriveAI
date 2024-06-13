using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public static class Clustering
{
    public static IReadOnlyList<Cluster> Cluster(ICollection<float[]> values, float d)
    {
        var clusters = new List<Cluster>();
        
        foreach (var value in values)
        {
            var cluster = clusters.FirstOrDefault(c => c.Distance(value) < d);
            if (cluster == null)
            {
                clusters.Add(new Cluster(value));
            }
            else
            {
                cluster.Add(value);
            }
        }

        return clusters;
    }
}

public class Cluster
{
    public float[] Center { get; private set; }
    public IReadOnlyList<float[]> Values => values;

    private readonly List<float[]> values = new();

    public Cluster(float[] center)
    {
        Center = center;
        values.Add(center);
    }

    public void Add(float[] value)
    {
        values.Add(value);
    }

    public float Distance(float[] value)
    {
        return Mathf.Sqrt(values.Aggregate(0f, (acc, val) =>
        {
            for (int i = 0; i < val.Length; i++)
            {
                acc += Mathf.Pow(val[i] - value[i], 2);
            }
            return acc;
        }));
    }
}
