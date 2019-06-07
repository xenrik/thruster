using System;
using System.Collections;
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
        foreach (SlicerDebug debug in sliceDebug(p)) {
            // Loop
        }
    }

    public IEnumerable<SlicerDebug> sliceDebug(Plane p) {

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
        foreach (SlicerDebug debug in fillHolesBowyerWatson(p)) {
            yield return debug;
        }

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

    private IEnumerable<SlicerDebug> fillHolesBowyerWatson(Plane p) {
        SlicerDebug debug;
        debug.vertices = new List<Vector3>(vertices);
        debug.posTriangles = new List<int>(posTriangles);
        debug.negTriangles = new List<int>(negTriangles);
        debug.colours = new List<Color>(colours);

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
        Triangle2D superTriangle = findSuperTriangle(min - new Vector2(1, 1), max + new Vector2(1, 1));
        Debug.Log("superTriangle: " + superTriangle);

        triangles.Add(superTriangle);

        // Calculate the triangle list
        HashSet<Edge2D> polygon = new HashSet<Edge2D>(new EdgeEquivalenceComparator());
        HashSet<Edge2D> badEdges = new HashSet<Edge2D>(new EdgeEquivalenceComparator());
        HashSet<Triangle2D> badTriangles = new HashSet<Triangle2D>();
        foreach (Vector2 point in perimiter) {
            Debug.Log("---");
            Debug.Log("Triangles: " + triangles.Count);

            badTriangles.Clear();
            foreach (Triangle2D tri in triangles) {
                if (tri.CircumcircleContains(point)) {
                    Debug.Log("badTriangle: " + tri + " (for point: " + point + ")");
                    badTriangles.Add(tri);
                    //break;
                    
                }
            }

            yield return fillDebug(triangles, p, planeRot, badTriangles, point);

            polygon.Clear();
            badEdges.Clear();
            foreach (Triangle2D tri in badTriangles) {
                triangles.Remove(tri);
                Debug.Log("Removed triangle: " + tri + " (" + triangles.Count + " left)");

                Edge2D ab = new Edge2D(tri.a, tri.b);
                if (!badEdges.Contains(ab)) {
                    if (polygon.Contains(ab)) {
                        polygon.Remove(ab);
                        badEdges.Add(ab);
                    } else {
                        polygon.Add(ab);
                    }
                }

                Edge2D bc = new Edge2D(tri.b, tri.c);
                if (!badEdges.Contains(bc)) {
                    if (polygon.Contains(bc)) {
                        polygon.Remove(bc);
                        badEdges.Add(bc);
                    } else {
                        polygon.Add(bc);
                    }
                }

                Edge2D ca = new Edge2D(tri.c, tri.a);
                if (!badEdges.Contains(ca)) {
                    if (polygon.Contains(ca)) {
                        polygon.Remove(ca);
                        badEdges.Add(ca);
                    } else {
                        polygon.Add(ca);
                    }
                }
            }

            int i = 0;
            Debug.Log("Unique Edges: ");
            foreach (Edge2D edge in polygon) {
                Debug.Log("- " + (++i) + " = " + edge + " (" + edge.GetHashCode() + ")");
            }

            i = 0;
            Debug.Log("Duplicate Edges: ");
            foreach (Edge2D edge in badEdges) {
                Debug.Log("- " + (++i) + " = " + edge + " (" + edge.GetHashCode() + ")");
            }

            foreach (Edge2D edge in polygon) {
                Triangle2D tri = new Triangle2D(edge.a, edge.b, point);
                Debug.Log("newTriangle: " + tri);
                triangles.Add(tri);
            }

            yield return fillDebug(triangles, p, planeRot);
        }

        // Cleanup
        badTriangles.Clear();
        foreach (Triangle2D tri in triangles) {
            if (tri.HasPoint(superTriangle.a) || tri.HasPoint(superTriangle.b) || 
                    tri.HasPoint(superTriangle.c)) {
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
            tri3d.a = (planeRot * tri3d.a) - (p.distance * p.normal);
            tri3d.ai = vertices.Count;

            tri3d.b = tri.b;
            tri3d.b = (planeRot * tri3d.b) - (p.distance * p.normal);
            tri3d.bi = vertices.Count + 1;

            tri3d.c = tri.c;
            tri3d.c = (planeRot * tri3d.c) - (p.distance * p.normal);
            tri3d.ci = vertices.Count + 2;

            //Debug.Log("--- " + tri + " => " + tri3d);

            vertices.Add(tri3d.a); colours.Add(Color.red);
            vertices.Add(tri3d.b); colours.Add(Color.green);
            vertices.Add(tri3d.c); colours.Add(Color.yellow);

            posTriangles.AddRange(new int[] { tri3d.ci, tri3d.bi, tri3d.ai });
            negTriangles.AddRange(new int[] { tri3d.ai, tri3d.bi, tri3d.ci });
        }
    }

    private SlicerDebug fillDebug(HashSet<Triangle2D> triangles, Plane p, Quaternion planeRot, HashSet<Triangle2D> badTriangles = null, Vector2? testPoint = null) {
        SlicerDebug debug;
        debug.perimiter = new HashSet<Vector3>();
        foreach (Edge e in edges) {
            Vector3 point = e.a;
            //point = (planeRot * point) - (p.distance * p.normal);
            debug.perimiter.Add(point);

            point = e.b;
            //point = (planeRot * point) - (p.distance * p.normal);
            debug.perimiter.Add(point);
        }

        if (badTriangles != null) {
            debug.badTriangles = new List<Vector3>();
            foreach (Triangle2D tri in badTriangles) {
                Vector3 point = tri.a;
                point = (planeRot * point) - (p.distance * p.normal);
                debug.badTriangles.Add(point);

                point = tri.b;
                point = (planeRot * point) - (p.distance * p.normal);
                debug.badTriangles.Add(point);

                point = tri.c;
                point = (planeRot * point) - (p.distance * p.normal);
                debug.badTriangles.Add(point);
            }
        } else {
            debug.badTriangles = null;
        }

        if (testPoint != null) {
            debug.testPoint = (Vector2)testPoint;
            debug.testPoint = (planeRot * debug.testPoint) - (p.distance * p.normal);
        } else {
            debug.testPoint = Vector3.positiveInfinity;
        }

        debug.vertices = new List<Vector3>();
        debug.posTriangles = new List<int>();
        debug.negTriangles = new List<int>();
        debug.colours = new List<Color>();

        /*
        debug.vertices = new List<Vector3>(vertices);
        debug.posTriangles = new List<int>(posTriangles);
        debug.negTriangles = new List<int>(negTriangles);
        debug.colours = new List<Color>(colours);
        */

        // Reorient and add to both the positivie and negative triangle lists
        foreach (Triangle2D tri in triangles) {
            Triangle tri3d = new Triangle();
            tri3d.a = tri.a;
            tri3d.a = (planeRot * tri3d.a) - (p.distance * p.normal);
            tri3d.ai = debug.vertices.Count;

            tri3d.b = tri.b;
            tri3d.b = (planeRot * tri3d.b) - (p.distance * p.normal);
            tri3d.bi = debug.vertices.Count + 1;

            tri3d.c = tri.c;
            tri3d.c = (planeRot * tri3d.c) - (p.distance * p.normal);
            tri3d.ci = debug.vertices.Count + 2;

            //Debug.Log("--- " + tri + " => " + tri3d);

            debug.vertices.Add(tri3d.a); debug.colours.Add(Color.red);
            debug.vertices.Add(tri3d.b); debug.colours.Add(Color.green);
            debug.vertices.Add(tri3d.c); debug.colours.Add(Color.yellow);

            debug.posTriangles.AddRange(new int[] { tri3d.ci, tri3d.bi, tri3d.ai });
            debug.negTriangles.AddRange(new int[] { tri3d.ai, tri3d.bi, tri3d.ci });
        }

        return debug;
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

        Vector2 ta = new Vector2(min.x - dx, min.y);
        Vector2 tb = new Vector2(max.x + dx, min.y);
        Vector2 tc = new Vector2(min.x + dx / 2, max.y + dy);

        return new Triangle2D(ta, tb, tc);
    }

    private struct Triangle2D {
        public Vector2 a { get; }
        public Vector2 b { get; }
        public Vector2 c { get; }

        public Vector2 circumcircleOrigin { get; private set; }
        public float circumcircleRadiusSq { get; private set; }

        private int hashCode;

        public Triangle2D(Vector2 a, Vector2 b, Vector2 c) {
            this.a = a;
            this.b = b;
            this.c = c;

            this.circumcircleOrigin = Vector2.zero;
            this.circumcircleRadiusSq = 0;

            int prime = 31;
            hashCode = Math.Round(this.a.x, 2).GetHashCode();
            hashCode = hashCode * prime + Math.Round(this.a.y, 2).GetHashCode();
            hashCode = hashCode * prime + Math.Round(this.b.x, 2).GetHashCode();
            hashCode = hashCode * prime + Math.Round(this.b.y, 2).GetHashCode();
            hashCode = hashCode * prime + Math.Round(this.c.x, 2).GetHashCode();
            hashCode = hashCode * prime + Math.Round(this.c.y, 2).GetHashCode();
        }

        public bool CircumcircleContains(Vector2 p) {
            if (circumcircleRadiusSq == 0) {
                calculateCircumcircle();
            }

            float dx = p.x - circumcircleOrigin.x;
            float dy = p.y - circumcircleOrigin.y;
            float rsq = dx*dx + dy*dy;

            return rsq < circumcircleRadiusSq;
            //return Mathf.Sqrt(rsq) < Mathf.Sqrt(circumcircleRadiusSq);
        }

        // Based on https://stackoverflow.com/a/9755252/2467874
        public bool HasPoint(Vector2 p) {
            if (a.Equals(p) || b.Equals(p) || c.Equals(p)) {
                return true;
            }
            /* 
            float aSideX = p.x - a.x;
            float aSideY = p.y - a.y;

            bool sideAB = (b.x - a.x) * aSideY - (b.y-a.y) * aSideX > 0;
            bool sideAC = (c.x - a.x) * aSideY - (c.y-a.y) * aSideX > 0;

            if (sideAB == sideAC) {
                return false;
            }

            float bSideX = p.y - b.x;
            float bSideY = p.y - b.y;
            bool sideCB = (c.x - b.x) * bSideY - (c.y-b.y) * bSideX > 0;

            if (sideCB == sideAB) {
                return false;
            }
            return true;
            */

            return false;
        }

        // Based on https://gist.github.com/mutoo/5617691#gistcomment-1329247
        private void calculateCircumcircle() {
            float ayby = Mathf.Abs(a.y - b.y);
            float bycy = Mathf.Abs(b.y - c.y);

            if (ayby < Mathf.Epsilon && bycy < Mathf.Epsilon) {
                // We have no size!
                return;
            }

            float xc = 0, yc = 0;

            if (ayby < Mathf.Epsilon) {
                float m = -((c.x - b.x) / (c.y - b.y));
                float mx = (b.x + c.x) / 2.0f;
                float my = (b.y + c.y) / 2.0f;

                xc = (b.x + a.y) / 2.0f;
                yc = m * (xc - mx) + my;
            } else if (bycy < Mathf.Epsilon) {
                float m = -((b.x - a.x) / (b.y - a.y));
                float mx = (a.x + b.x) / 2.0f;
                float my = (a.y + b.y) / 2.0f;

                xc = (c.x + b.x) / 2.0f;
                yc = m * (xc - mx) + my;
            } else {
                float m1 = -((b.x - a.x) / (b.y - a.y));
                float m1x = (a.x + b.x) / 2.0f;
                float m1y = (a.y + b.y) / 2.0f;

                float m2 = -((c.x - b.x) / (c.y - b.y));
                float m2x = (b.x + c.x) / 2.0f;
                float m2y = (b.y + c.y) / 2.0f;

                xc = (m1 * m1x - m2 * m2x + m2y - m1y) / (m1 - m2);
                yc = (ayby > bycy) ? m1 * (xc - m1x) + m1y : m2 * (xc - m2x) + m2y;
            }

            circumcircleOrigin = new Vector2(xc, yc);

            float dx = b.x - xc;
            float dy = b.y - yc;
            circumcircleRadiusSq = dx*dx + dy*dy;
        }

        public override string ToString() {
            return $"[{a},{b},{c}]";
        }

        public override bool Equals(object obj) {
            if (!(obj is Triangle2D)) {
                return false;
            }

            Triangle2D other = (Triangle2D)obj;
            return a.Equals(other.a) && b.Equals(other.b) && c.Equals(other.c);
        }

        public override int GetHashCode() {
            return hashCode;
        }
    }

    private struct Edge2D {
        public Vector2 a { get; }
        public Vector2 b { get; }
        public int hashCode;

        public Edge2D(Vector2 a, Vector2 b) {
            this.a = a;
            this.b = b;

            int prime = 31;
            int hashCodeA = Math.Round(this.a.x, 2).GetHashCode();
            hashCodeA = hashCodeA * prime + Math.Round(this.a.y, 2).GetHashCode();

            int hashCodeB = Math.Round(this.b.x, 2).GetHashCode();
            hashCodeB = hashCodeB * prime + Math.Round(this.b.y, 2).GetHashCode();

            hashCode = hashCodeA ^ hashCodeB;
        }

        public override string ToString() {
            return $"[{a},{b}]";
        }

        public override bool Equals(object obj) {
            if (!(obj is Edge2D)) {
                return false;
            }

            Edge2D other = (Edge2D)obj;
            float d = (a.x - b.x) + (a.y - b.y);
            return d < Mathf.Epsilon;
        }

        public override int GetHashCode() {
            return hashCode;
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

    public struct SlicerDebug {
        public HashSet<Vector3> perimiter;

        public List<Color> colours;
        public List<Vector3> vertices;

        public List<int> posTriangles;
        public List<int> negTriangles;

        public List<Vector3> badTriangles;
        public Vector3 testPoint;
    }

    private class EdgeEquivalenceComparator : IEqualityComparer<Edge2D> {
        public bool Equals(Edge2D a, Edge2D b) {
            float d = (a.a.x - b.a.x) + (a.a.y - b.a.y) + 
                (a.b.x - b.b.x) + (a.b.y - b.b.y);
            if (d < Mathf.Epsilon) {
                return true;
            }

            d = (a.a.x - b.b.x) + (a.a.y - b.b.y) + 
                (a.b.x - b.a.x) + (a.b.y - b.a.y);
            if (d < Mathf.Epsilon) {
                return true;
            }

            return false;
        }

        public int GetHashCode(Edge2D obj) {
            return obj.GetHashCode();   
        }
    }
}