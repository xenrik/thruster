using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

public class Slicer {

    public Mesh posMesh;
    public Mesh negMesh;

    private Mesh mesh;
    private bool optimise;
    private bool checkForHoles;

    private object slicePos;
    private object sliceNeg;

    private Ray ray;

    private List<Color> colours = new List<Color>();
    private List<Vector3> vertices = new List<Vector3>();

    private List<int> posTriangles = new List<int>();
    private List<int> negTriangles = new List<int>();
    private List<Edge3D> edges = new List<Edge3D>();

    public Slicer(Mesh mesh, bool optimise = false, bool checkForHoles = false) {
        this.mesh = mesh;
        this.optimise = optimise;
        this.checkForHoles = checkForHoles;
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
        fillHolesBowyerWatson(p);

        // TODO - Optimise the mesh
        // if (optimise) {
        //    ...
        // }

        //UnityEngine.Debug.Log("Split " + count + " triangles");
        //UnityEngine.Debug.Log("Original Mesh: " + existingVertices.Length + " vertices, " + triangles.Length/3 + " triangles");

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

    private Vector3 getCut(Vector3 a, Vector3 b, Plane p) {
        ray.origin = a;
        ray.direction = b - a;
        float length;

        bool result = p.Raycast(ray, out length);
        if (!result && length == 0) {
            // Something went wrong!
            UnityEngine.Debug.Log("Unexpectedly didn't intersect the plane?");
            return Vector3.zero;
        }

        return ray.GetPoint(length);
    }

    private void fillHolesBowyerWatson(Plane p) {
        // Rotate the edges to be on a horizontal plane at the origin
        Quaternion up = Quaternion.LookRotation(Vector3.up);
        Quaternion planeRot = Quaternion.LookRotation(p.normal);
        Quaternion rot = up * Quaternion.Inverse(planeRot);
        Quaternion irot = Quaternion.Inverse(rot);
    
        HashSet<Edge2D> perimiter = new HashSet<Edge2D>();
        foreach (Edge3D e in edges) {
            Vector3 a = rot * (e.a + (p.distance * p.normal));
            Vector3 b = rot * (e.b + (p.distance * p.normal));

            perimiter.Add(new Edge2D(a,b));
        }
         
        BowyerWatsonFill fill = new BowyerWatsonFill(perimiter, checkForHoles);
        fill.Fill();

        // Reorient and add to both the positivie and negative triangle lists
        foreach (Triangle2D tri in fill.Triangles) {
            Triangle3D tri3d = new Triangle3D();
            tri3d.a = tri.a.v;
            tri3d.a = (irot * tri3d.a) - (p.distance * p.normal);
            tri3d.ai = vertices.Count;

            tri3d.b = tri.b.v;
            tri3d.b = (irot * tri3d.b) - (p.distance * p.normal);
            tri3d.bi = vertices.Count + 1;

            tri3d.c = tri.c.v;
            tri3d.c = (irot * tri3d.c) - (p.distance * p.normal);
            tri3d.ci = vertices.Count + 2;

            vertices.Add(tri3d.a); colours.Add(Color.red);
            vertices.Add(tri3d.b); colours.Add(Color.green);
            vertices.Add(tri3d.c); colours.Add(Color.yellow);

            posTriangles.AddRange(new int[] { tri3d.ai, tri3d.bi, tri3d.ci });
            negTriangles.AddRange(new int[] { tri3d.ci, tri3d.bi, tri3d.ai });
        }
    }
}