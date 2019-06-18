using UnityEngine;
using System;
using System.Collections.Generic;

public class Point2D {
    private static Point2D _zero = new Point2D(0, 0);
    public static Point2D zero { get { return _zero; } }

    private const long PRECISION = 4;

    public float x { get; private set; }
    public float y { get; private set; }

    public Vector3 v { get; private set; }

    private int hashCode;

    public Point2D(Vector3 p) : this(p.x, p.z) {
        this.v = p;
    }

    public Point2D(float x, float y) {
        if (PRECISION > 0) {
            float FACTOR = Mathf.Pow(10, PRECISION);
            long xl = (long)(x * FACTOR);
            this.x = xl / FACTOR;

            long yl = (long)(y * FACTOR);
            this.y = yl / FACTOR;
        } else {
            this.x = x;
            this.y = y;
        }

        this.v = new Vector3(this.x, 0, this.y);
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
        return $"[{x:F4},{y:F4}]";
    }
    
    public static implicit operator Point2D(Vector2 point) {
        return new Point2D(point.x, point.y);
    }
    
    public static implicit operator Point2D(Vector3 point) {
        return new Point2D(point);
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