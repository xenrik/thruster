using UnityEngine;

using System;
using System.Collections.Generic;

public class Edge2D {
    public Point2D a { get; }
    public Point2D b { get; }

    public int hashCode;

    private bool calculatedBounds = false;
    private float minx, miny, maxx, maxy;

    public Edge2D(Point2D a, Point2D b) {
        this.a = a;
        this.b = b;
        
        // We want our hashcode to be commutative (although Equals still cares about the order)
        hashCode = a.GetHashCode() ^ b.GetHashCode();
    }

    /**
     * Returns true if the edge intersects the line
     * from a to b
     */
    public bool Intersects(Point2D c, Point2D d) {
        if (!calculatedBounds) {
            minx = Mathf.Min(a.x, b.x);
            maxx = Mathf.Max(a.x, b.x);
            miny = Mathf.Min(a.y, b.y);
            maxy = Mathf.Max(a.y, b.y);

            calculatedBounds = true;
        }

        // Check we are within the bounds of the edge
        float test_minx = Mathf.Min(c.x, d.x); 
        if (test_minx > maxx) {
            return false;
        }

        float test_maxx = Mathf.Max(c.x, d.x);
        if (test_maxx < minx) {
            return false;
        }

        float test_miny = Mathf.Min(c.y, d.y);
        if (test_miny > maxy) {
            return false;
        }

        float test_maxy = Mathf.Max(c.y, d.y);
        if (test_maxy < miny) {
            return false;
        }

        // Check if we are on different sides of the edge
        int o1 = Orientation(a, b, c);
        int o2 = Orientation(a, b, d);
        int o3 = Orientation(c, d, a);
        int o4 = Orientation(c, d, b);

        if (o1 != o2 && o3 != o4) {
            return true;
        }

        // Check we intersect
        if (o1 == 0 && OnSegment(c, maxx, minx, maxy, miny)) { // a, c, b)) {
            return true;
        }
        if (o2 == 0 && OnSegment(d, maxx, minx, maxy, miny)) { // a, d, b)) {
            return true;
        }
        if (o3 == 0 && OnSegment(a, test_maxx, test_minx, test_maxy, test_miny)) { // c, a, d)) {
            return true;
        }
        if (o4 == 0 && OnSegment(b, test_maxx, test_minx, test_maxy, test_miny)) { // c, b, d)) {
            return true;
        }

        // We don't intersect
        return false;
    }

    private bool OnSegment(Point2D p, float max_qrx, float min_qrx, float max_qry, float min_qry) {
        return p.x <= max_qrx && p.y >= min_qrx && p.y <= max_qry && p.y >= min_qry;
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