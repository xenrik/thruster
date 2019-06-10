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
    private List<Edge3D> edges = new List<Edge3D>();
    private HashSet<Edge2D> perimiter = new HashSet<Edge2D>();

    private bool debugging = false;

    public Slicer(Mesh mesh, bool optimise = false, FillType fillType = FillType.GRID) {
        this.mesh = mesh;
        this.optimise = optimise;
    }

    public void slice(Plane p) {
        foreach (SlicerDebug debug in doSliceDebug(p)) {
            // Loop
        }
    }

    public IEnumerable<SlicerDebug> sliceDebug(Plane p) {
        debugging = true;
        foreach (SlicerDebug debug in doSliceDebug(p)) {
            yield return debug;
        }
    }

    private IEnumerable<SlicerDebug> doSliceDebug(Plane p) {
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

        Triangle3D t = new Triangle3D();
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

        // Remove the filled holes that were inside


        // TODO - Optimise
        // if (optimise) {
        //    ...
        // }

        //Log("Split " + count + " triangles");
        //Log("Original Mesh: " + existingVertices.Length + " vertices, " + triangles.Length/3 + " triangles");

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

        //Log("Positive Mesh: " + posMesh.vertices.Length + " vertices, " + posMesh.triangles.Length/3 + " triangles");
        //Log("Negative Mesh: " + negMesh.vertices.Length + " vertices, " + negMesh.triangles.Length/3 + " triangles");
    }

    private void splitTriangle(Triangle3D t, Plane p) {
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
            edges.Add(new Edge3D(cut1, cut2, t.normal));

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
            edges.Add(new Edge3D(cut1, cut2, t.normal));

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
            edges.Add(new Edge3D(cut1, cut2, t.normal));

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

    private void Log(String msg) {
        if (debugging) {
            Debug.Log(msg);
        }
    }

    private Vector3 getCut(Vector3 a, Vector3 b, Plane p) {
        ray.origin = a;
        ray.direction = b - a;
        float length;

        bool result = p.Raycast(ray, out length);
        if (!result && length == 0) {
            // Something went wrong!
            Log("Unexpectedly didn't intersect the plane?");
            return Vector3.zero;
        }

        return ray.GetPoint(length);
    }

    private IEnumerable<SlicerDebug> fillHolesBowyerWatson(Plane p) {
        // Rotate the edges to be on a horizontal plane at the origin
        Quaternion up = Quaternion.Euler(Vector3.up);
        Quaternion planeRot = Quaternion.LookRotation(p.normal);
        Quaternion inorm = Quaternion.Inverse(planeRot);
        Quaternion rot = up * inorm;

        perimiter.Clear();

        Point2D min = new Point2D(float.MaxValue, float.MaxValue);
        Point2D max = new Point2D(float.MinValue, float.MinValue);
        foreach (Edge3D e in edges) {
            addEdge(e, rot, p, ref min, ref max);
        }
         
        Log($"Perimiter has {perimiter.Count} points");

        // Find and add the super-triangle
        HashSet<Triangle2D> triangles = new HashSet<Triangle2D>();
        Triangle2D superTriangle = findSuperTriangle(min - new Point2D(1, 1), max + new Point2D(1, 1));
        Log("superTriangle: " + superTriangle);

        triangles.Add(superTriangle);

        // Calculate the triangle list
        HashSet<Edge2D> polygon = new HashSet<Edge2D>(new Edge2D.EquivalentComparator());
        HashSet<Edge2D> badEdges = new HashSet<Edge2D>(new Edge2D.EquivalentComparator());
        HashSet<Triangle2D> badTriangles = new HashSet<Triangle2D>();
        foreach (Point2D point in perimiter) {
            Log("---");
            Log("Triangles: " + triangles.Count);

            badTriangles.Clear();
            foreach (Triangle2D tri in triangles) {
                if (tri.CircumcircleContains(point)) {
                    Log("badTriangle: " + tri + " (for point: " + point + ")");
                    badTriangles.Add(tri);
                    //break;
                    
                }
            }

            yield return fillDebug(triangles, p, planeRot, badTriangles, point);

            polygon.Clear();
            badEdges.Clear();
            foreach (Triangle2D tri in badTriangles) {
                triangles.Remove(tri);
                Log("Removed triangle: " + tri + " (" + triangles.Count + " left)");

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
            Log("Unique Edges: ");
            foreach (Edge2D edge in polygon) {
                Log("- " + (++i) + " = " + edge + " (" + edge.GetHashCode() + ")");
            }

            i = 0;
            Log("Duplicate Edges: ");
            foreach (Edge2D edge in badEdges) {
                Log("- " + (++i) + " = " + edge + " (" + edge.GetHashCode() + ")");
            }

            foreach (Edge2D edge in polygon) {
                Triangle2D tri = new Triangle2D(edge.a, edge.b, point);
                Log("newTriangle: " + tri);
                triangles.Add(tri);
            }

            yield return fillDebug(triangles, p, planeRot);
        }

        // Cleanup
        badTriangles.Clear();
        foreach (Triangle2D tri in triangles) {
            // If the triangle has a corner of the super triangle, remove it
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
            Triangle3D tri3d = new Triangle3D();
            tri3d.a = tri.a;
            tri3d.a = (planeRot * tri3d.a) - (p.distance * p.normal);
            tri3d.ai = vertices.Count;

            tri3d.b = tri.b;
            tri3d.b = (planeRot * tri3d.b) - (p.distance * p.normal);
            tri3d.bi = vertices.Count + 1;

            tri3d.c = tri.c;
            tri3d.c = (planeRot * tri3d.c) - (p.distance * p.normal);
            tri3d.ci = vertices.Count + 2;

            // Find the edge for a-b
            Edge3D edge = findEdge(tri.a, tri.b);

            //Log("--- " + tri + " => " + tri3d);

            vertices.Add(tri3d.a); colours.Add(Color.red);
            vertices.Add(tri3d.b); colours.Add(Color.green);
            vertices.Add(tri3d.c); colours.Add(Color.yellow);

            posTriangles.AddRange(new int[] { tri3d.ci, tri3d.bi, tri3d.ai });
            negTriangles.AddRange(new int[] { tri3d.ai, tri3d.bi, tri3d.ci });
        }
    }

    private Edge3D findEdge(Point2D a, Point2D b) {
        foreach (Point2D p in perimiter) {
            if (p.Equals(a)) {
                for (Edge)
            }
        }
    }

    private SlicerDebug fillDebug(HashSet<Triangle2D> triangles, Plane p, Quaternion planeRot, HashSet<Triangle2D> badTriangles = null, Point2D? testPoint = null) {
        SlicerDebug debug;
        debug.perimiter = new HashSet<Vector3>();
        foreach (Point2D point in perimiter) {
            Vector3 point3d = point;
            point3d = (planeRot * point3d) - (p.distance * p.normal);

            debug.perimiter.Add(point3d);
        }

        int i = 0;
        debug.allTriangles = new List<Triangle3D>();
        foreach (Triangle2D tri in triangles) {
            Triangle3D tri3d = new Triangle3D();
            tri3d.color = Triangle3D.GetColor(i++);

            tri3d.a = tri.a;
            tri3d.a = (planeRot * tri3d.a) - (p.distance * p.normal);

            tri3d.b = tri.b;
            tri3d.b = (planeRot * tri3d.b) - (p.distance * p.normal);

            tri3d.c = tri.c;
            tri3d.c = (planeRot * tri3d.c) - (p.distance * p.normal);

            tri3d.circumcircleOrigin = (planeRot * (Vector3)tri.circumcircleOrigin) - (p.distance * p.normal);
            tri3d.circumcircleRadius = Mathf.Sqrt(tri.circumcircleRadiusSq);
            debug.allTriangles.Add(tri3d);
        }

        debug.badTriangles = new List<Triangle3D>();
        if (badTriangles != null) {
            foreach (Triangle2D tri in badTriangles) {
                Triangle3D tri3d = new Triangle3D();
                tri3d.color = Triangle3D.GetColor(i++);

                Vector3 point = tri.a;
                point = (planeRot * point) - (p.distance * p.normal);
                tri3d.a = point;

                point = tri.b;
                point = (planeRot * point) - (p.distance * p.normal);
                tri3d.b = point;

                point = tri.c;
                point = (planeRot * point) - (p.distance * p.normal);
                tri3d.c = point;

                tri3d.circumcircleOrigin = (planeRot * (Vector3)tri.circumcircleOrigin) - (p.distance * p.normal);
                tri3d.circumcircleRadius = Mathf.Sqrt(tri.circumcircleRadiusSq);

                debug.badTriangles.Add(tri3d);
            }
        }

        if (testPoint != null) {
            debug.testPoint = (Vector3)testPoint;
            debug.testPoint = (planeRot * debug.testPoint) - (p.distance * p.normal);
        } else {
            debug.testPoint = Vector3.positiveInfinity;
        }

        return debug;
    }

    private Edge2D addEdge(Edge3D edge, Quaternion rot, Plane p, ref Point2D min, ref Point2D max) {
        Point2D a = new Point2D((rot * (edge.a) - (p.distance * p.normal)));
        Point2D b = new Point2D((rot * (edge.b) - (p.distance * p.normal)));
        Point2D n = new Point2D(rot * (edge.normal));
        
        Edge2D edge2d = new Edge2D(a, b, n);
        perimiter.Add(edge2d);

        min.x = Mathf.Min(min.x, edge2d.a.x, edge2d.b.x);
        min.y = Mathf.Min(min.y, edge2d.a.y, edge2d.b.y);
        max.x = Mathf.Max(max.x, edge2d.a.x, edge2d.b.x);
        max.y = Mathf.Max(max.y, edge2d.a.y, edge2d.b.y);

        return edge2d;
    }

    /**
     * Find a triangle that encompases the given min and max bounding points
     */
    private Triangle2D findSuperTriangle(Point2D min, Point2D max) {
        float dx = max.x - min.x;
        float dy = max.y - min.y;

        Point2D ta = new Point2D(min.x - dx, min.y);
        Point2D tb = new Point2D(max.x + dx, min.y);
        Point2D tc = new Point2D(min.x + dx / 2, max.y + dy);

        return new Triangle2D(ta, tb, tc);
    }

    public struct SlicerDebug {
        public HashSet<Vector3> perimiter;

        public List<Triangle3D> allTriangles;

        public List<Triangle3D> badTriangles;
        public Vector3 testPoint;
    }
}