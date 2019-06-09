using UnityEngine;

public struct Triangle3D {
    private static Color[] colors = new Color[] {
        Color.blue, Color.green, Color.yellow, Color.cyan, Color.magenta
    };

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

    public Vector3 circumcircleOrigin;
    public float circumcircleRadius;

    public Color color;

    public static Color GetColor(int i) {
        return colors[i % colors.Length];
    }

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
        return $"[{a.ToString("F2")},{b.ToString("F2")},{c.ToString("F2")}]";
    }

    public override int GetHashCode() {
        int hashCode = a.GetHashCode();
        hashCode = hashCode * 31 + b.GetHashCode();
        hashCode = hashCode * 31 + c.GetHashCode();

        return hashCode;
    }

    public override bool Equals(object obj) {
        if (!(obj is Triangle3D)) {
            return false;
        }

        Triangle3D other = (Triangle3D)obj;
        return a.Equals(other.a) && b.Equals(other.b) && c.Equals(other.c);
    }
} 