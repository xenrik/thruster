using System.Collections.Generic;
using UnityEngine;

public class Slicer {
    private Mesh mesh;

    private object slicePos;
    private object sliceNeg;

    public Slicer(Mesh mesh) {
        this.mesh = mesh;
    }

    public void slice(Plane p) {
        Vector3[] vertices = mesh.vertices;
        int[] triangles = mesh.triangles;
        
        Vector3?[] posVertices = new Vector3?[vertices.Length];
        int?[] posTriangles = new int?[triangles.Length];

        Vector3?[] negVertices = new Vector3?[vertices.Length];
        int?[] negTriangles = new int?[triangles.Length];
        
        for (int tri = 0; tri < triangles.Length; tri += 3) {
            bool side1 = p.GetSide(vertices[tri]);
            bool side2 = p.GetSide(vertices[tri+1]);
            bool side3 = p.GetSide(vertices[tri+2]);

            // If all the points of the triangle are on the same side,
            // we don't need to split it.
            if (side1 == side2 == side3) {
                if (side1) {
                    posVertices[tri] = vertices[tri];
                    posVertices[tri+1] = vertices[tri+1];
                    posVertices[tri+2] = vertices[tri+2];
                } else {
                    negVertices[tri] = vertices[tri];
                    negVertices[tri+1] = vertices[tri+1];
                    negVertices[tri+2] = vertices[tri+2];
                }
            } else {
                // Split the triangle
            }
        }
    }
}