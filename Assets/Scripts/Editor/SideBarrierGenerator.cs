using Ivankarez.RacetrackGenerator;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class SideBarrierGenerator : EditorWindow
{
    const float barrierHeight = 1.5f;
    private TrackData trackData;

    [MenuItem("Window/Barrier Generator")]
    public static void ShowWindow()
    {
        GetWindow(typeof(SideBarrierGenerator), false, "Barrier Generator");
    }

    private void OnGUI()
    {
        trackData = (TrackData)EditorGUILayout.ObjectField("Target Track Data:", trackData, typeof(TrackData), false);

        GUILayout.Space(10);
        var buttonStyle = new GUIStyle(GUI.skin.button)
        {
            fixedHeight = 25,
        };
        if (GUILayout.Button("Generate mesh", buttonStyle))
        {
            GenerateSideBarriers();
        }
    }

    private void GenerateSideBarriers()
    {
        GenerateBarriersForPoints(trackData.leftLine, "track-left-barriers", true);
        GenerateBarriersForPoints(trackData.rightLine, "track-right-barriers", false);
    }

    private void GenerateBarriersForPoints(Vector3[] points, string name, bool revertOrder)
    {
        var mesh = new Mesh();
        var vertices = new List<Vector3>();
        var triangles = new List<int>();
        var uvs = new List<Vector2>();

        var bottomLeft = points[^1];
        var topLeft = bottomLeft + Vector3.up * barrierHeight;
        vertices.Add(bottomLeft);
        uvs.Add(new Vector2(0, 0));
        vertices.Add(topLeft);
        uvs.Add(new Vector2(0, 1));

        for (int i = 0; i < points.Length; i++)
        {
            var bottomRight = points[i];
            var topRight = bottomRight + Vector3.up * barrierHeight;

            vertices.Add(bottomRight);
            vertices.Add(topRight);

            var vertCount = vertices.Count;
            if (revertOrder)
            {
                triangles.Add(vertCount - 4);
                triangles.Add(vertCount - 2);
                triangles.Add(vertCount - 3);
                triangles.Add(vertCount - 2);
                triangles.Add(vertCount - 1);
                triangles.Add(vertCount - 3);
            }
            else
            {
                triangles.Add(vertCount - 4);
                triangles.Add(vertCount - 3);
                triangles.Add(vertCount - 2);
                triangles.Add(vertCount - 2);
                triangles.Add(vertCount - 3);
                triangles.Add(vertCount - 1);
            }

            uvs.Add(new Vector2(1, 0));
            uvs.Add(new Vector2(1, 1));

            bottomLeft = bottomRight;
            topLeft = topRight;
        }

        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.uv = uvs.ToArray();
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        AssetDatabase.CreateAsset(mesh, $"Assets/{name}.asset");
    }
}
