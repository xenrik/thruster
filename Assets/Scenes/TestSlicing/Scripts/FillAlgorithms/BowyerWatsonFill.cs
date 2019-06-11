using UnityEngine;
using System.Collections.Generic;

public class BowyerWatsonFill {
    public ICollection<Triangle2D> Triangles {
        get {
            return triangles;
        }
    }

    private ICollection<Point2D> perimiter;

    private HashSet<Triangle2D> triangles = new HashSet<Triangle2D>();
    private Point2D min;
    private Point2D max;

    private Dictionary<Edge2D,int> polygon = new Dictionary<Edge2D, int>(new Edge2D.EquivalentComparator());
    private HashSet<Triangle2D> badTriangles = new HashSet<Triangle2D>();

    public BowyerWatsonFill(ICollection<Point2D> perimiter) {
        this.perimiter = perimiter;

        foreach (Point2D point in perimiter) {
            min.x = Mathf.Min(min.x, point.x);
            min.y = Mathf.Min(min.y, point.y);
            max.x = Mathf.Max(max.x, point.x);
            max.y = Mathf.Max(max.y, point.y);
        }
    }

    public void Fill() {
        Triangle2D superTriangle = findSuperTriangle(min - new Point2D(1, 1), max + new Point2D(1, 1));
        triangles.Add(superTriangle);

        foreach (Point2D point in perimiter) {
            processPoint(point);
        }

        // Cleanup
        badTriangles.Clear();
        foreach (Triangle2D tri in triangles) {
            // If the triangle has a corner of the super triangle, remove it
            if (tri.HasPoint(superTriangle.a) || tri.HasPoint(superTriangle.b) || 
                    tri.HasPoint(superTriangle.c)) {
                badTriangles.Add(tri);
            }

            // For an arbitrary line from the centre of the triangle, 
            // see how many times we cross the perimiter. 
            // If this is even or zero, we are outside the perimiter.            
        }

        foreach (Triangle2D tri in badTriangles) {
            triangles.Remove(tri);
        }
    }
    
    /**
     * Find a triangle that encompases the given min and max bounding points
     */
    private Triangle2D findSuperTriangle(Point2D min, Point2D max) {
        float dx = max.x - min.x;
        float dy = max.y - min.y;

        Point2D ta = new Point2D(min.x - dx, min.y);
        Point2D tb = new Point2D(max.x + dx, min.y);
        Point2D tc = new Point2D(min.x + dx / 2, max.y + dy);

        return new Triangle2D(ta, tb, tc);
    }

    private void processPoint(Point2D point) {
        badTriangles.Clear();
        foreach (Triangle2D tri in triangles) {
            if (tri.CircumcircleContains(point)) {
                badTriangles.Add(tri);
            }
        }

        polygon.Clear();
        foreach (Triangle2D tri in badTriangles) {
            triangles.Remove(tri);

            Edge2D ab = new Edge2D(tri.a, tri.b);
            if (polygon.ContainsKey(ab)) {
                polygon[ab] = polygon[ab] + 1;
            } else {
                polygon[ab] = 1;
            }

            Edge2D bc = new Edge2D(tri.b, tri.c);
            if (polygon.ContainsKey(bc)) {
                polygon[bc] = polygon[bc] + 1;
            } else {
                polygon[bc] = 1;
            }

            Edge2D ca = new Edge2D(tri.c, tri.a);
            if (polygon.ContainsKey(ca)) {
                polygon[ca] = polygon[ca] + 1;
            } else {
                polygon[ca] = 1;
            }
        }

        foreach (KeyValuePair<Edge2D,int> edge in polygon) {
            if (edge.Value == 1) {
                Triangle2D tri = new Triangle2D(edge.Key.a, edge.Key.b, point);
                triangles.Add(tri);
            }
        }
    }
}