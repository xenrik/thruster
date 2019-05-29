using System.Collections.Generic;
using UnityEngine;

public class Slicer {
    private Mesh mesh;
    private bool optimise;

    private object slicePos;
    private object sliceNeg;

    private Ray ray;

    public Mesh posMesh;
    public Mesh negMesh;

    public Slicer(Mesh mesh, bool optimise = false) {
        this.mesh = mesh;
        this.optimise = optimise;
    }

    public void slice(Plane p) {
        Vector3[] vertices = mesh.vertices;
        int[] triangles = mesh.triangles;
        
        List<Vector3> newVertices = new List<Vector3>();
        List<int> posTriangles = new List<int>();
        List<int> negTriangles = new List<int>();
        Triangle t = new Triangle();
        
        for (int tri = 0; tri < triangles.Length; tri += 3) {
            t.initialise(vertices, triangles[tri], triangles[tri + 1], triangles[tri + 2], p);

            // If all the points of the triangle are on the same side,
            // we don't need to split it.
            if (t.aSide == t.bSide == t.cSide) {
                if (t.aSide) {
                    posTriangles.AddRange(new int[] { tri, tri+1, tri+2 });
                } else {
                    negTriangles.AddRange(new int[] { tri, tri+1, tri+2 });
                }
            } else {
                // Split the triangle
                splitTriangle(t, p, newVertices, posTriangles, negTriangles, vertices.Length);
            }
        }

        // TODO - Optimise
        // if (optimise) {
        //    ...
        // }

        Vector3[] allVertices = new Vector3[vertices.Length + newVertices.Count];
        System.Array.Copy(vertices, allVertices, vertices.Length);
        newVertices.CopyTo(0, allVertices, vertices.Length, newVertices.Count);

        Mesh posMesh = new Mesh();
        posMesh.vertices = allVertices;
        posMesh.triangles = posTriangles.ToArray();
        posMesh.RecalculateBounds();
        posMesh.RecalculateNormals();

        Mesh negMesh = new Mesh();
        negMesh.vertices = allVertices;
        negMesh.triangles = negTriangles.ToArray();
        negMesh.RecalculateBounds();
        negMesh.RecalculateNormals();
    }

    private void splitTriangle(Triangle t, Plane p, List<Vector3> newVertices, List<int> posTriangles, List<int> negTriangles, int vertexOffset) {
        // Find which edges need splitting
        bool splitAB = t.aSide != t.bSide;
        bool splitBC = t.bSide != t.cSide;
        bool splitCA = t.cSide != t.aSide;

        int cut1i = vertexOffset + newVertices.Count;
        int cut2i = cut1i + 1;

        if (splitAB && splitBC) {
            newVertices.Add(getCut(t.a, t.b, p)); // cut1
            newVertices.Add(getCut(t.b, t.c, p)); // cut2

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
            newVertices.Add(getCut(t.a, t.b, p)); // cut1
            newVertices.Add(getCut(t.c, t.a, p)); // cut2

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
            newVertices.Add(getCut(t.b, t.c, p)); // cut1
            newVertices.Add(getCut(t.c, t.a, p)); // cut2

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
        if (!p.Raycast(ray, out length)) {
            // Something went wrong!
            Debug.Log("Unexpectedly didn't intersect the plane?");
            return Vector3.zero;
        }

        return ray.GetPoint(length);
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
        }
    }
}