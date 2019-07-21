using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshBuilder : MonoBehaviour {
    Vector3[] newVertices;
    Vector2[] newUV;
    int[] newTriangles;

    void Start()
    {
        newVertices = new Vector3[] {
            new Vector3(0, 0, 0),
            new Vector3(0, 0, 1),
            new Vector3(1, 0, 0),
            new Vector3(1, 0, 1),
            new Vector3(0, 1, 0),
            new Vector3(0, 1, 1),
            new Vector3(1, 1, 0),
            new Vector3(1, 1, 1),
        };

        var triangles = new List<int>();
        //triangles.Add(CreateTriangleIndexes(int ));


        Mesh mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
        mesh.vertices = newVertices;
        mesh.uv = newUV;
        mesh.triangles = newTriangles;
    }

    private static int[] CreateTriangleIndexes(params int[] sqIndexes)
    {
        return new int[] {
            sqIndexes[0],
            sqIndexes[1],
            sqIndexes[2],
            sqIndexes[0],
            sqIndexes[2],
            sqIndexes[3],
        };
    }
}
