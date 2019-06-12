using UnityEngine;
using System;
using System.Collections.Generic;

public struct Point2D {
    private static Point2D _zero = new Point2D(0, 0);
    public static Point2D zero { get { return _zero; } }

    private const int PRECISION = 4;

    public float x;
    public float y;

    private int hashCode;

    public Point2D(Vector3 p) : this(p.x, p.y) {
    }

    public Point2D(float x, float y) {
        if (PRECISION > 0) {
            this.x = (float)Math.Round(x, PRECISION);
            this.y = (float)Math.Round(y, PRECISION);
        } else {
            this.x = x;
            this.y = y;
        }

        this.hashCode = (int)(this.x + (31 * this.y));
    }

    public override int GetHashCode() {
        return hashCode;
    }

    public override bool Equals(object obj) {
        if (!(obj is Point2D)) {
            return false;
        }

        Point2D other = (Point2D)obj;
        return x == other.x && y == other.y;
    }

    public override string ToString() {
        return $"[{x:F2},{y:F2}]";
    }
    
    public static implicit operator Vector3(Point2D point) {
        return new Vector3(point.x, point.y, 0);
    }

    public static Point2D operator -(Point2D a, Point2D b) {
        return new Point2D(a.x - b.x, a.y - b.y);
    }

    public static Point2D operator +(Point2D a, Point2D b) {
        return new Point2D(a.x + b.x, a.y + b.y);
    }
}