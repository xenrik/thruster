using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * Functions for dealing with triangles
 */
public class Triangle {
    /**
     * Returns true if the specified ray intersects with the triangle defined by the
     * supplied vectors.
     * 
     * Purloined from: http://answers.unity.com/answers/861741/view.html
     */
    public static bool Intersects(Vector3 p1, Vector3 p2, Vector3 p3, Ray ray) {
        // Vectors from p1 to p2/p3 (edges)
        Vector3 e1 = p2 - p1;
        Vector3 e2 = p3 - p1;

        // Calculate determinant
        Vector3 p = Vector3.Cross(ray.direction, e2);
        float det = Vector3.Dot(e1, p);

        // If determinant is near zero, the ray lines in a plane of the triangle, otherwise it doesn't.
        if (det > -float.Epsilon && det < float.Epsilon) {
            return false;
        }

        float invDet = 1.0f / det;

        // Calculate distance from p1 to ray origin
        Vector3 t = ray.origin - p1;

        // Calculate u and check for ray hit
        float u = Vector3.Dot(t, p) * invDet;
        if (u < 0 || u > 1) {
            return false;
        }

        // Calculate v and check for ray hit
        Vector3 q = Vector3.Cross(t, e1);
        float v = Vector3.Dot(ray.direction, q) * invDet;
        if (v < 0 || (u + v) > 1) {
            return false;
        }

        if ((Vector3.Dot(e2, q) * invDet) > float.Epsilon) {
            // Intersection!
            return true;
        }

        // No intersection
        return false;
    } 
}
