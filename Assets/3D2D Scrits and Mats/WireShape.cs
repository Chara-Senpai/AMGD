using System.Collections.Generic;
using UnityEngine;

public class WireShape : MonoBehaviour
{
    public enum ShapeType { Cube, Pyramid, Sphere }

    [SerializeField] private ShapeType shape = ShapeType.Cube;
    [SerializeField] private Material lineMat;
    [SerializeField] private Vector3 position = Vector3.zero;
    [SerializeField] private float scale = 1;
    [Range(1, 500)] [SerializeField] private float focalLength = 20f;

    [Header("Rotation (degrees per second)")]
    [SerializeField] private float xSpeed = 40f;
    [SerializeField] private float ySpeed = 60f;

    public float xRotation, yRotation;

    [Header("Sphere")]
    [Range(6, 32)] [SerializeField] private int sphereSegments = 8;

    private Vector3[] verts;
    private int[][] faces;

    private readonly List<int> visibleEdges = new();
    private readonly List<Vector3> ActualPosition = new();
    private Mesh mesh;

    #region Unity Functions

    private void Awake()
    {
        switch (shape)
        {
            case ShapeType.Cube: BuildCube(); break;
            case ShapeType.Pyramid: BuildPyramid(); break;
            case ShapeType.Sphere: BuildSphere(Mathf.Max(6, sphereSegments)); break;
        }
        OrientFaces();

        mesh = new Mesh();
        gameObject.AddComponent<MeshFilter>().mesh = mesh;
        gameObject.AddComponent<MeshRenderer>().material = lineMat;
    }

    private void Update()
    {
        xRotation += xSpeed * Time.deltaTime;
        yRotation += ySpeed * Time.deltaTime;
    }

    private void LateUpdate()
    {
        ActualPosition.Clear();
        foreach (var point in verts)
            ActualPosition.Add(Project(point));

        mesh.vertices = ActualPosition.ToArray();
        visibleEdges.Clear();

        foreach (var f in faces)
        {
            if (!FacesCamera(ActualPosition[f[0]], ActualPosition[f[1]], ActualPosition[f[2]])) continue;
            for (int i = 0; i < f.Length; i++)
            {
                visibleEdges.Add(f[i]);
                visibleEdges.Add(f[(i + 1) % f.Length]);
            }
        }

        mesh.SetIndices(visibleEdges, MeshTopology.Lines, 0);
        mesh.RecalculateBounds();
    }

    #endregion

    #region Shape Builders

    private void BuildCube()
    {
        verts = new Vector3[]
        {
            new(-1, -1, -1), new(1, -1, -1), new(1, 1, -1), new(-1, 1, -1),
            new(-1, -1, 1),  new(1, -1, 1),  new(1, 1, 1),  new(-1, 1, 1)
        };
        faces = new[]
        {
            new[] { 0, 1, 2, 3 }, new[] { 5, 4, 7, 6 }, new[] { 4, 0, 3, 7 },
            new[] { 1, 5, 6, 2 }, new[] { 3, 2, 6, 7 }, new[] { 0, 4, 5, 1 }
        };
    }

    private void BuildPyramid()
    {
        verts = new Vector3[]
        {
            new(-1, -1, -1), new(1, -1, -1), new(1, -1, 1), new(-1, -1, 1),
            new(0, 1, 0)
        };
        faces = new[]
        {
            new[] { 0, 1, 2, 3 },
            new[] { 0, 1, 4 }, new[] { 1, 2, 4 }, new[] { 2, 3, 4 }, new[] { 3, 0, 4 }
        };
    }

    private void BuildSphere(int segments)
    {
        int stacks = segments;
        int slices = segments * 2;
        var v = new List<Vector3> { new(0, 1, 0) };

        for (int i = 1; i < stacks; i++)
        {
            float phi = Mathf.PI * i / stacks;
            for (int j = 0; j < slices; j++)
            {
                float theta = 2 * Mathf.PI * j / slices;
                v.Add(new Vector3(Mathf.Sin(phi) * Mathf.Cos(theta),
                                  Mathf.Cos(phi),
                                  Mathf.Sin(phi) * Mathf.Sin(theta)));
            }
        }
        v.Add(new Vector3(0, -1, 0));
        int bottom = v.Count - 1;

        int Ring(int ring, int j) => 1 + (ring - 1) * slices + (j % slices);

        var f = new List<int[]>();
        for (int j = 0; j < slices; j++)
            f.Add(new[] { 0, Ring(1, j), Ring(1, j + 1) });

        for (int i = 1; i < stacks - 1; i++)
            for (int j = 0; j < slices; j++)
                f.Add(new[] { Ring(i, j), Ring(i + 1, j), Ring(i + 1, j + 1), Ring(i, j + 1) });

        for (int j = 0; j < slices; j++)
            f.Add(new[] { Ring(stacks - 1, j), bottom, Ring(stacks - 1, j + 1) });

        verts = v.ToArray();
        faces = f.ToArray();
    }

    private void OrientFaces()
    {
        Vector3 center = Vector3.zero;
        foreach (var p in verts) center += p;
        center /= verts.Length;

        foreach (var f in faces)
        {
            Vector3 a = verts[f[0]], b = verts[f[1]], c = verts[f[2]];
            Vector3 normal = Vector3.Cross(b - a, c - a);

            Vector3 centroid = Vector3.zero;
            foreach (int idx in f) centroid += verts[idx];
            centroid /= f.Length;

            if (Vector3.Dot(normal, centroid - center) > 0)
                System.Array.Reverse(f);
        }
    }

    #endregion

    #region Helper

    private Vector2 Project(Vector3 point)
    {
        point *= .5f;

        float xr = xRotation * Mathf.Deg2Rad;
        float y1 = point.y * Mathf.Cos(xr) - point.z * Mathf.Sin(xr);
        float z1 = point.y * Mathf.Sin(xr) + point.z * Mathf.Cos(xr);
        point = new Vector3(point.x, y1, z1);

        float yr = yRotation * Mathf.Deg2Rad;
        float x2 = point.x * Mathf.Cos(yr) - point.z * Mathf.Sin(yr);
        float z2 = point.x * Mathf.Sin(yr) + point.z * Mathf.Cos(yr);
        point = new Vector3(x2, point.y, z2);

        point += position;

        float p = focalLength / (focalLength + point.z);
        return new Vector2(point.x, point.y) * scale * p;
    }

    private bool FacesCamera(Vector2 a, Vector2 b, Vector2 c)
    {
        return (b - a).x * (c - a).y - (b - a).y * (c - a).x > 0;
    }

    #endregion
}