using UnityEngine;
using System;

public class Triangle2D {
    public Point2D a { get; }
    public Point2D b { get; }
    public Point2D c { get; }

    public Vector2 circumcircleOrigin { get; private set; }
    public float circumcircleRadiusSq { get; private set; }

    private int hashCode = -1;
    private bool calculatedCirumcircle = false;

    public Triangle2D(Point2D a, Point2D b, Point2D c) {
        this.a = a;
        this.b = b;
        this.c = c;

        this.circumcircleOrigin = Vector2.zero;
        this.circumcircleRadiusSq = 0;
        
        //calculateCircumcircle();
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
        if (hashCode == -1) {
            hashCode = this.a.GetHashCode();
            hashCode = hashCode * 31 + this.b.GetHashCode();
            hashCode = hashCode * 31 + this.c.GetHashCode();
        }
        
        return hashCode;
    }

    public bool CircumcircleContains(Point2D p) {
        if (!calculatedCirumcircle) {
            calculateCircumcircle();
        }

        float dx = p.x - circumcircleOrigin.x;
        float dy = p.y - circumcircleOrigin.y;
        float rsq = dx*dx + dy*dy;

        return rsq < circumcircleRadiusSq;
        //return Mathf.Sqrt(rsq) < Mathf.Sqrt(circumcircleRadiusSq);
    }

    public Point2D CentroidPoint() {
        return new Point2D((a.x + b.x + c.x) / 3.0f, (a.y + b.y + c.y) / 3.0f);
    }

    public bool HasPoint(Point2D p) {
        return a.Equals(p) || b.Equals(p) || c.Equals(p);
    }

    // Based on https://gist.github.com/mutoo/5617691#gistcomment-1329247
    private void calculateCircumcircle() {
        float fabsy1y2 = Mathf.Abs(a.y - b.y);
        float fabsy2y3 = Mathf.Abs(b.y - c.y);

        if (fabsy1y2 < Mathf.Epsilon && fabsy2y3 < Mathf.Epsilon) {
            // We have no size!
            return;
        }

        float xc = 0, yc = 0;

        if (fabsy1y2 < Mathf.Epsilon) {
            float m = -((c.x - b.x) / (c.y - b.y));
            float mx = (b.x + c.x) / 2.0f;
            float my = (b.y + c.y) / 2.0f;

            xc = (b.x + a.x) / 2.0f;
            yc = m * (xc - mx) + my;
        } else if (fabsy2y3 < Mathf.Epsilon) {
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
            yc = (fabsy1y2 > fabsy2y3) ? m1 * (xc - m1x) + m1y : m2 * (xc - m2x) + m2y;
        }

        circumcircleOrigin = new Vector2(xc, yc);

        float dx = b.x - xc;
        float dy = b.y - yc;
        circumcircleRadiusSq = dx*dx + dy*dy;
        calculatedCirumcircle = true;
    }
}