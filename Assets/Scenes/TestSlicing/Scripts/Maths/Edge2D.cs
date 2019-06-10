using UnityEngine;

using System;
using System.Collections.Generic;

public struct Edge2D {
    public Point2D a { get; }
    public Point2D b { get; }
    public Point2D normal { get; }

    public int hashCode;

    public Edge2D(Point2D a, Point2D b, Point2D normal) {
        this.a = a;
        this.b = b;
        this.normal = normal;
        
        hashCode = a.GetHashCode() ^ b.GetHashCode();
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