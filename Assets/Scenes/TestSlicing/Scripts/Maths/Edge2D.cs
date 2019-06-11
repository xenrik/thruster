using UnityEngine;

using System;
using System.Collections.Generic;

public struct Edge2D {
    public Point2D a { get; }
    public Point2D b { get; }

    public int hashCode;

    public Edge2D(Point2D a, Point2D b) {
        this.a = a;
        this.b = b;
        
        hashCode = a.GetHashCode() ^ b.GetHashCode();
    }

    /**
     * Returns true if the edge intersects the line
     * from a to b
     */
    public bool Intersects(Point2D c, Point2D d) {
        int o1 = Orientation(a, b, c);
        int o2 = Orientation(a, b, d);
        int o3 = Orientation(c, d, a);
        int o4 = Orientation(c, d, b);

        if (o1 != o2 && o3 != o4) {
            return true;
        }

        if (o1 == 0 && OnSegment(a, c, b)) {
            return true;
        }
        if (o2 == 0 && OnSegment(a, d, b)) {
            return true;
        }
        if (o3 == 0 && OnSegment(c, a, d)) {
            return true;
        }
        if (o4 == 0 && OnSegment(c, b, d)) {
            return true;
        }

        return false;
    }

    private bool OnSegment(Point2D p, Point2D q, Point2D r) {
        return (q.x <= Mathf.Max(p.x, r.x) && q.x >= Mathf.Min(p.x, r.x) &&
            q.y <= Mathf.Max(p.y, r.y) && q.y >= Mathf.Min(p.y, r.y));
    }

    private int Orientation(Point2D p, Point2D q, Point2D r) {
        float val = (q.y - p.y) * (r.x - q.x) - (q.x - p.x) * (r.y - q.y);
        if (val == 0) {
            return 0;
        } else {
            return (val > 0) ? 1 : 2;
        }
    }

    public override string ToString() {
        return $"[{a},{b}]";
    }

    public override bool Equals(object obj) {
        if (!(obj is Edge2D)) {
            return false;
        }

        Edge2D other = (Edge2D)obj;
        return a.Equals(other.a) && b.Equals(other.b);
    }

    public override int GetHashCode() {
        return hashCode;
    }

    public class EquivalentComparator : IEqualityComparer<Edge2D> {
        public bool Equals(Edge2D x, Edge2D y) {
            if (x.Equals(y)) {
                return true;
            }

            return x.a.Equals(y.b) && x.b.Equals(y.a);
        }

        public int GetHashCode(Edge2D obj) {
            return obj.GetHashCode();
        }
    }
}