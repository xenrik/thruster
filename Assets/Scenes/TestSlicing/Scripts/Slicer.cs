using System.Collections.Generic;
using UnityEngine;

public class Slicer {

    public enum FillType { GRID };

    public Mesh posMesh;
    public Mesh negMesh;

    private Mesh mesh;
    private bool optimise;

    private object slicePos;
    private object sliceNeg;

    private Ray ray;

    private List<Color> colours = new List<Color>();
    private List<Vector3> vertices = new List<Vector3>();

    private List<int> posTriangles = new List<int>();
    private List<int> negTriangles = new List<int>();
    private List<Edge> edges = new List<Edge>();

    public Slicer(Mesh mesh, bool optimise = false, FillType fillType = FillType.GRID) {
        this.mesh = mesh;
        this.optimise = optimise;
    }

    public void slice(Plane p) {

        Vector3[] existingVertices = mesh.vertices;
        int[] triangles = mesh.triangles;
        Vector3[] normals = mesh.normals;

        vertices.Clear();
        vertices.AddRange(existingVertices);

        colours.Clear();
        colours.AddRange(mesh.colors);
        while (colours.Count < vertices.Count) {
            colours.Add(Color.white);
        }

        posTriangles.Clear();
        negTriangles.Clear();
        edges.Clear();

        Triangle t = new Triangle();
        int count = 0;
        
        // Slice triangles at the intersection with the plane
        for (int tri = 0; tri < triangles.Length; tri += 3) {
            t.initialise(existingVertices, triangles[tri], triangles[tri + 1], triangles[tri + 2], p);

            // If all the points of the triangle are on the same side,
            // we don't need to split it.
            if (t.aSide == t.bSide && t.aSide == t.cSide) {
                if (t.aSide) {
                    posTriangles.AddRange(new int[] { t.ai, t.bi, t.ci });
                } else {
                    negTriangles.AddRange(new int[] { t.ai, t.bi, t.ci });
                }
            } else {
                // Split the triangle
                splitTriangle(t, p);
                ++count;
            }
        }

        // Fill the holes
        fillHolesGrid(p);

        // TODO - Optimise
        // if (optimise) {
        //    ...
        // }

        //Debug.Log("Split " + count + " triangles");
        //Debug.Log("Original Mesh: " + existingVertices.Length + " vertices, " + triangles.Length/3 + " triangles");

        posMesh = new Mesh();
        posMesh.vertices = vertices.ToArray();
        posMesh.triangles = posTriangles.ToArray();
        posMesh.colors = colours.ToArray();
        posMesh.RecalculateBounds();
        posMesh.RecalculateNormals();

        negMesh = new Mesh();
        negMesh.vertices = vertices.ToArray();
        negMesh.triangles = negTriangles.ToArray();
        negMesh.colors = colours.ToArray();
        negMesh.RecalculateBounds();
        negMesh.RecalculateNormals();

        //Debug.Log("Positive Mesh: " + posMesh.vertices.Length + " vertices, " + posMesh.triangles.Length/3 + " triangles");
        //Debug.Log("Negative Mesh: " + negMesh.vertices.Length + " vertices, " + negMesh.triangles.Length/3 + " triangles");
    }

    private void splitTriangle(Triangle t, Plane p) {
        // Find which edges need splitting
        bool splitAB = t.aSide != t.bSide;
        bool splitBC = t.bSide != t.cSide;
        bool splitCA = t.cSide != t.aSide;

        int cut1i = vertices.Count;
        int cut2i = cut1i + 1;

        if (splitAB && splitBC) {
            Vector3 cut1 = getCut(t.a, t.b, p);
            Vector3 cut2 = getCut(t.b, t.c, p);
            if (cut1 == Vector3.zero || cut2 == Vector3.zero) { return; }

            vertices.Add(cut1); vertices.Add(cut2);
            edges.Add(new Edge(cut1, cut2, t.normal));

            colours.Add(Color.red);
            colours.Add(Color.blue);

            if (t.aSide) {
                posTriangles.AddRange(new int[] { t.ai, cut1i, cut2i });
                negTriangles.AddRange(new int[] { cut1i, t.bi, cut2i });
                posTriangles.AddRange(new int[] { cut2i, t.ci, t.ai });
            } else {
                negTriangles.AddRange(new int[] { t.ai, cut1i, cut2i });
                posTriangles.AddRange(new int[] { cut1i, t.bi, cut2i });
                negTriangles.AddRange(new int[] { cut2i, t.ci, t.ai });
            }
        } else if (splitAB && splitCA) {
            Vector3 cut1 = getCut(t.a, t.b, p);
            Vector3 cut2 = getCut(t.c, t.a, p);
            if (cut1 == Vector3.zero || cut2 == Vector3.zero) { return; }

            vertices.Add(cut1); vertices.Add(cut2);
            edges.Add(new Edge(cut1, cut2, t.normal));

            colours.Add(Color.yellow);
            colours.Add(Color.green);

            if (t.aSide) {
                posTriangles.AddRange(new int[] { t.ai, cut1i, cut2i });
                negTriangles.AddRange(new int[] { cut1i, t.bi, cut2i });
                negTriangles.AddRange(new int[] { t.bi, t.ci, cut2i });
            } else {
                negTriangles.AddRange(new int[] { t.ai, cut1i, cut2i });
                posTriangles.AddRange(new int[] { cut1i, t.bi, cut2i });
                posTriangles.AddRange(new int[] { t.bi, t.ci, cut2i });
            }
        } else {
            // Must be splitBC && splitCA
            Vector3 cut1 = getCut(t.b, t.c, p);
            Vector3 cut2 = getCut(t.c, t.a, p);
            if (cut1 == Vector3.zero || cut2 == Vector3.zero) { return; }

            vertices.Add(cut1); vertices.Add(cut2);
            edges.Add(new Edge(cut1, cut2, t.normal));

            colours.Add(Color.cyan);
            colours.Add(Color.magenta);

            if (t.aSide) {
                posTriangles.AddRange(new int[] { t.ai, cut1i, cut2i });
                posTriangles.AddRange(new int[] { t.ai, t.bi, cut1i });
                negTriangles.AddRange(new int[] { cut1i, t.ci, cut2i });                
            } else {
                negTriangles.AddRange(new int[] { t.ai, cut1i, cut2i });
                negTriangles.AddRange(new int[] { t.ai, t.bi, cut1i });
                posTriangles.AddRange(new int[] { cut1i, t.ci, cut2i });
            }
        }
    }

    private Vector3 getCut(Vector3 a, Vector3 b, Plane p) {
        ray.origin = a;
        ray.direction = b - a;
        float length;

        bool result = p.Raycast(ray, out length);
        if (!result && length == 0) {
            // Something went wrong!
            Debug.Log("Unexpectedly didn't intersect the plane?");
            return Vector3.zero;
        }

        return ray.GetPoint(length);
    }

    private void fillHolesGrid(Plane p) {
        // Rotate the edges to be on a horizontal plane at the origin
        Quaternion up = Quaternion.Euler(Vector3.up);
        Quaternion inorm = Quaternion.Inverse(Quaternion.LookRotation(p.normal));
        Quaternion rot = up * inorm;

        List<Vector2> perimiter = new List<Vector2>();
        foreach (Edge e in edges) {
            Vector3 rotated = rot * (e.a - (p.distance * p.normal));
            perimiter.Add(rotated);            
        }
        
    }


    private struct Triangle {
        public int ai;
        public Vector3 a;
        public bool aSide;

        public int bi;
        public Vector3 b;
        public bool bSide;

        public int ci;
        public Vector3 c;
        public bool cSide;

        public Vector3 normal;

        public void initialise(Vector3[] vertices, int ai, int bi, int ci, Plane p) {
            this.ai = ai;
            a = vertices[ai];
            aSide = p.GetSide(a);

            this.bi = bi;
            b = vertices[bi];
            bSide = p.GetSide(b);

            this.ci = ci;
            c = vertices[ci];
            cSide = p.GetSide(c);

            normal = Vector3.Cross(b - a, b - c).normalized;
        }
    }

    private struct Edge {
        public Vector3 a;
        public Vector3 b;

        public Vector3 direction;
        public Vector3 normal;

        public Edge(Vector3 a, Vector3 b, Vector3 normal) {
            this.a = a;
            this.b = b;
            this.normal = normal;

            direction = (b - a).normalized;
        }

        public int getSide(Vector3 p) {
            float d = Vector3.Dot(Vector3.Cross(p - a, direction), normal);
            return d < 0 ? -1 : d > 0 ? 1 : 0;
        }
    }
}