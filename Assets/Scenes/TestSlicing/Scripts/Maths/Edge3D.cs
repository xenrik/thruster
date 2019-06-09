using UnityEngine;

public struct Edge3D {
    public Vector3 a;
    public Vector3 b;

    public Vector3 direction;
    public Vector3 normal;

    public Edge3D(Vector3 a, Vector3 b, Vector3 normal) {
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