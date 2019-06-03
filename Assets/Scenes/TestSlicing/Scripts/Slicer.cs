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
        fillHolesBowyerWatson(p);

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

            colours.Add(Color.blue);
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

            colours.Add(Color.blue);
            colours.Add(Color.blue);

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

            colours.Add(Color.blue);
            colours.Add(Color.blue);

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

    private void fillHolesBowyerWatson(Plane p) {
        // Rotate the edges to be on a horizontal plane at the origin
        Quaternion up = Quaternion.Euler(Vector3.up);
        Quaternion planeRot = Quaternion.LookRotation(p.normal);
        Quaternion inorm = Quaternion.Inverse(planeRot);
        Quaternion rot = up * inorm;

        HashSet<Vector2> perimiter = new HashSet<Vector2>();
        Vector2 min = new Vector2(float.MaxValue, float.MaxValue);
        Vector2 max = new Vector2(float.MinValue, float.MinValue);
        foreach (Edge e in edges) {
            rotateAndAdd(e.a, rot, p, perimiter, ref min, ref max);
            rotateAndAdd(e.b, rot, p, perimiter, ref min, ref max);
        }

        // Find and add the super-triangle
        HashSet<Triangle2D> triangles = new HashSet<Triangle2D>();
        Triangle2D superTriangle = findSuperTriangle(min, max);
        Debug.Log("superTriangle: " + superTriangle);

        triangles.Add(superTriangle);

        // Calculate the triangle list
        HashSet<Edge2D> polygon = new HashSet<Edge2D>();
        HashSet<Triangle2D> badTriangles = new HashSet<Triangle2D>();
        foreach (Vector2 point in perimiter) {
            badTriangles.Clear();
            foreach (Triangle2D tri in triangles) {
                if (tri.Contains(point)) {
                    badTriangles.Add(tri);
                    break;
                }
            }

            polygon.Clear();
            foreach (Triangle2D tri in badTriangles) {
                triangles.Remove(tri);

                polygon.Add(new Edge2D(tri.a, tri.b));
                polygon.Add(new Edge2D(tri.b, tri.c));
                polygon.Add(new Edge2D(tri.c, tri.a));
            }

            foreach (Edge2D edge in polygon) {
                triangles.Add(new Triangle2D(edge.a, edge.b, point));
            }
        }

        // Cleanup
        badTriangles.Clear();
        foreach (Triangle2D tri in triangles) {
            if (tri.Contains(superTriangle.a) || tri.Contains(superTriangle.b) || 
                    tri.Contains(superTriangle.c)) {
                badTriangles.Add(tri);
            }
        }
        foreach (Triangle2D tri in badTriangles) {
            triangles.Remove(tri);
        }

        // Reorient and add to both the positivie and negative triangle lists
        foreach (Triangle2D tri in triangles) {
            Triangle tri3d = new Triangle();
            tri3d.a = tri.a;
            tri3d.a = (planeRot * tri3d.a) + (p.distance * p.normal);
            tri3d.ai = vertices.Count;

            tri3d.b = tri.b;
            tri3d.b = (planeRot * tri3d.b) + (p.distance * p.normal);
            tri3d.bi = vertices.Count + 1;

            tri3d.c = tri.c;
            tri3d.c = (planeRot * tri3d.c) + (p.distance * p.normal);
            tri3d.ci = vertices.Count + 2;

            Debug.Log("--- " + tri + " => " + tri3d);

            vertices.Add(tri3d.a); colours.Add(Color.red);
            vertices.Add(tri3d.b); colours.Add(Color.green);
            vertices.Add(tri3d.c); colours.Add(Color.yellow);

            posTriangles.AddRange(new int[] { tri3d.ci, tri3d.bi, tri3d.ai });
            negTriangles.AddRange(new int[] { tri3d.ai, tri3d.bi, tri3d.ci });
        }
    }

    private void rotateAndAdd(Vector3 v, Quaternion rot, Plane p, HashSet<Vector2> perimiter, ref Vector2 min, ref Vector2 max) {
        Vector3 rotated = rot * (v - (p.distance * p.normal));
        perimiter.Add(rotated);            
        
        min.x = Mathf.Min(min.x, rotated.x);
        min.y = Mathf.Min(min.y, rotated.y);
        max.x = Mathf.Max(max.x, rotated.x);
        max.y = Mathf.Max(max.y, rotated.y);
    }

    /**
     * Find a triangle that encompases the given min and max bounding points
     */
    private Triangle2D findSuperTriangle(Vector2 min, Vector2 max) {
        float dx = max.x - min.x;
        float dy = max.y - min.y;

        Triangle2D triangle;
        triangle.a.x = min.x - dx;
        triangle.a.y = min.y;

        triangle.b.x = max.x + dx;
        triangle.b.y = min.y;

        triangle.c.x = min.x + dx / 2;
        triangle.c.y = max.y + dy;

        return triangle;
    }

    private struct Triangle2D {
        public Vector2 a;
        public Vector2 b;
        public Vector2 c;
        
        public Triangle2D(Vector2 a, Vector2 b, Vector2 c) {
            this.a = a;
            this.b = b;
            this.c = c;
        }

        public bool Contains(Vector2 p) {
            return false;
        }

        public override string ToString() {
            return $"[{a},{b},{c}]";
        }
    }

    public struct Edge2D {
        public Vector2 a;
        public Vector2 b;

        public Edge2D(Vector2 a, Vector2 b) {
            this.a = a;
            this.b = b;
        }

        public override string ToString() {
            return $"[{a},{b}]";
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

        public override string ToString() {
            return $"[{a},{b},{c}]";
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