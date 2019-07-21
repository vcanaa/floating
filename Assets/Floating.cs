using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Floating : MonoBehaviour {
    public Material white;

    MeshFilter meshFilter;
    Vector3[] worldVertices;
    Rigidbody body;
    int[] triangles;

    public float floatIntensity;
    public float g;

    public int debugTriangleIndex = 0;

    Mesh mesh;

    Vector3 initialPosition;
    Quaternion initialRotation;

    // Use this for initialization
    void Start () {
        initialPosition = transform.position;
        initialRotation = transform.rotation;
        body = GetComponent<Rigidbody>();
        meshFilter = GetComponent<MeshFilter>();
        worldVertices = new Vector3[meshFilter.mesh.vertices.Length];
        triangles = meshFilter.mesh.triangles;

        mesh = new Mesh();
        mesh.vertices = new Vector3[] {
            new Vector3(0.5f, 0, 0),
            new Vector3(-0.5f, 0, 0),
            new Vector3(0.5f, 0, 1),
            new Vector3(0, -0.5f, 1),
        };

        mesh.triangles = new int[] {
            0, 1, 2,
            2, 1, 0,
            2, 1, 3,
            3, 1, 2,
        };
    }
	
	// Update is called once per frame
	void Update () {
        if (Input.GetKeyDown(KeyCode.A)) {
            body.position = initialPosition;
            body.velocity = Vector3.zero;
            body.rotation = initialRotation;
        }

        CalculateWorldVertices();

		for (int i = 0; i < triangles.Length; i += 3) {
            //if (i != (debugTriangleIndex * 3) % triangles.Length) continue;

            int min = 0;
            for (int j = 0; j < 3; j++) {
                if (worldVertices[triangles[i + j]].y < worldVertices[triangles[i + min]].y) {
                    min = j;
                }
            }

            Vector3 v0 = worldVertices[triangles[i + min]];
            // The whole triangle is above surface.
            if (v0.y >= 0) {
                continue;
            }

            //Debug.Log("triangle: " + i / 3);
            DebugCross(Color.blue, v0);

            Vector3 v1 = worldVertices[triangles[i + (min + 1) % 3]];
            Vector3 m01 = v1;
            bool cut01 = false;
            if (v1.y > 0) {
                m01 = GetSurfacePoint(v0, v1);
                cut01 = true;
            }

            DebugCross(Color.green, v1);

            Vector3 v2 = worldVertices[triangles[i + (min + 2) % 3]];
            Vector3 m02 = v2;
            bool cut02 = false;
            if (v2.y > 0) {
                m02 = GetSurfacePoint(v0, v2);
                cut02 = true;
            }

            DebugCross(Color.yellow, v2);

            // DrawPoly(Color.green, v0, v1, v2);

            if (cut01) {
                if (cut02) {
                    DebugCross(Color.cyan, m01);
                    DebugCross(Color.magenta, m02);
                    ApplyFloatForceTriangle(v0, m01, m02, 1);
                } else {
                    Vector3 m12 = GetSurfacePoint(v1, v2);
                    DebugCross(Color.cyan, m01);
                    DebugCross(Color.magenta, m12);
                    ApplyFloatForceQuadGeneric(v0, v2, m12, m01, -1);
                }
            } else {
                if (cut02) {
                    Vector3 m12 = GetSurfacePoint(v1, v2);
                    DebugCross(Color.cyan, m02);
                    DebugCross(Color.magenta, m12);
                    ApplyFloatForceQuadGeneric(v0, v1, m12, m02, 1);
                } else {
                    ApplyFloatForceTriangleGeneric(v0, v1, v2);
                }
            }
        }

        body.AddForce(Vector3.down * body.mass * g);
    }

    private void CalculateWorldVertices()
    {
        for (int i =0; i < worldVertices.Length; i++) {
            worldVertices[i] = transform.TransformPoint(meshFilter.mesh.vertices[i]);
        }
    }

    private static Vector3 Avg(params Vector3[] points)
    {
        Vector3 avg = Vector3.zero;
        foreach (Vector3 p in points) {
            avg += p;
        }

        return avg / points.Length;
    }

    private void ApplyFloatForceQuadGeneric(Vector3 v0, Vector3 v1, Vector3 v2, Vector3 v3, int inversion)
    {
        Vector3 m = GetMidPoint(v0, v3, v1.y);

        ApplyFloatForceQuad(m, v1, v2, v3, inversion);
        ApplyFloatForceTriangle(v0, v1, m, inversion);
    }

    private void ApplyFloatForceTriangleGeneric(Vector3 v0, Vector3 v1, Vector3 v2)
    {
        int inversion = 1;
        if (v1.y > v2.y) {
            inversion = -1;
            Vector3 aux = v1;
            v1 = v2;
            v2 = aux;
        }

        Vector3 m = GetMidPoint(v0, v2, v1.y);

        ApplyFloatForceTriangle(v2, m, v1, inversion);
        ApplyFloatForceTriangle(v0, v1, m, inversion);
    }

    private void ApplyFloatForceQuad(Vector3 v0, Vector3 v1, Vector3 v2, Vector3 v3, int inversion)
    {
        float p0 = GetPressure(v0);
        float p1 = GetPressure(v2);

        Vector3 B = v1 - v0;
        if (B.magnitude == 0) return;

        Vector3 R = v3 - v0;
        if (R.magnitude == 0) return;

        float b0 = (v1 - v0).magnitude;
        float b1 = (v3 - v2).magnitude;
        Vector3 m0 = (v0 + v1) / 2;
        Vector3 m1 = (v2 + v3) / 2;

        DebugCross(Color.red, m0);
        DebugCross(Color.red, m1);


        //Graphics.DrawMeshNow(
        //    mesh,
        //    Matrix4x4.Scale(new Vector3(0.1f, (m1 - m0).magnitude, 0)) * 
        //    Matrix4x4.LookAt(m0, m1, Camera.allCameras[0].transform.position - m0) *
        //    Matrix4x4.Translate(new Vector3()),
        //    // Quaternion.LookRotation(m1-m0, Camera.allCameras[0].transform.position - m0), 
        //    0);
        //Debug.DrawLine(m0, m1, Color.red);

        float l = GetL(b0, b1, p0, p1);
        float F = GetF(b0, b1, p0, p1, getH(B, R));

        Vector3 point = Vector3.Lerp(m0, m1, l);

        Vector3 dir = Vector3.Cross(R, B) / R.magnitude / B.magnitude * inversion;

        DebugPoly(Color.magenta, v0, v1, v2, v3);
        ApplyFloatForce(dir.y * F, point);

    }

    private void ApplyFloatForceTriangle(Vector3 v0, Vector3 v1, Vector3 v2, int inversion)
    {
        float p0 = GetPressure(v0);
        float p1 = GetPressure(v1);

        Vector3 B = v2 - v1; // base
        if (B.magnitude == 0) return;

        Vector3 R = v0 - v1; // ramp
        if (R.magnitude == 0) return;

        float b0 = 0;
        float b1 = B.magnitude;
        Vector3 m = (v1 + v2) / 2;

        DebugCross(Color.red, m);

        //Debug.DrawLine(v0, m, Color.red);

        float l = GetL(b0, b1, p0, p1);
        float F = GetF(b0, b1, p0, p1, getH(B, R)) * floatIntensity;

        Vector3 point = Vector3.Lerp(v0, m, l);

        Vector3 dir = Vector3.Cross(R, B) / B.magnitude / R.magnitude * inversion;

        DebugPoly(Color.magenta, v0, v1, v2);
        ApplyFloatForce(dir.y * F, point);
    }

    private void ApplyFloatForce(float floatForce, Vector3 position)
    {
        Vector3 force = Vector3.up * floatForce;
        body.AddForceAtPosition(force, position);
        Debug.DrawLine(position, position + force * 4);
        DebugCross(Color.white, position);
    }

    private static float getH(Vector3 B, Vector3 R)
    {
        return Vector3.Cross(B, R).magnitude / B.magnitude;
    }

    private static float GetL(float b0, float b1, float p0, float p1)
    {
        float db = b1 - b0;
        float dp = p1 - p0;
        float b0p0 = b0 * p0;
        float cross = b0 * dp + db * p0;
        float dbdp = db * dp;
        return (6 * b0p0 + 4 * cross + 3 * dbdp) /
              (12 * b0p0 + 6 * cross + 4 * dbdp);
    }

    private static float GetF(float b0, float b1, float p0, float p1, float h)
    {
        float db = b1 - b0;
        float dp = p1 - p0;
        float b0p0 = b0 * p0;
        float cross = b0 * dp + db * p0;
        float dbdp = db * dp;
        return (b0p0 + cross / 2 + dbdp / 3) * h;
    }

    private float GetPressure(Vector3 v)
    {
        return -v.y;
    }

    //private Vector3 CalculateFloatForce(Vector3 a, Vector3 b, Vector3 c)
    //{
    //    float avgP = (a.y + b.y + c.y) / 3;
    //    Vector3 d1 = b - a;
    //    Vector3 d2 = c - a;
    //    return Vector3.Cross(d1, d2) * avgP;
    //}

    private static Vector3 GetSurfacePoint(Vector3 a, Vector3 b)
    {
        return GetMidPoint(a, b, 0);
    }

    private static Vector3 GetMidPoint(Vector3 a, Vector3 b, float y)
    {
        float x = (y - a.y) / (b.y - a.y);
        return Vector3.Lerp(a, b, x);
    }


    private static void DebugPoly(Color color, params Vector3[] points)
    {
        if (points.Length < 2) {
            return;
        }

        if (points.Length == 2) {
            Debug.DrawLine(points[0], points[1], color);
            return;
        }

        for (int i = 0; i < points.Length - 1; i++) {
            Debug.DrawLine(points[i], points[i + 1], color);
        }
        Debug.DrawLine(points.Last(), points[0], color);
    }

    private static void DebugCross(Color color, Vector3 pos)
    {
        //float size = 0.02f;
        //Debug.DrawLine(pos + Vector3.up * size, pos - Vector3.up * size, color);
        //Debug.DrawLine(pos + Vector3.right * size, pos - Vector3.right * size, color);
        //Debug.DrawLine(pos + Vector3.forward * size, pos - Vector3.forward * size, color);
    }
}
