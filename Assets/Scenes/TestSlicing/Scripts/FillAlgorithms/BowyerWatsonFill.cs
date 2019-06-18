using UnityEngine;
using System.Collections.Generic;

public class BowyerWatsonFill {
    public ICollection<Triangle2D> Triangles {
        get {
            return triangles;
        }
    }

    private ICollection<Edge2D> perimiter;

    private HashSet<Triangle2D> triangles = new HashSet<Triangle2D>();
    private HashSet<Triangle2D> newTriangles = new HashSet<Triangle2D>();
    private Vector2 min;
    private Vector2 max;

    private Dictionary<Edge2D,int> polygon = new Dictionary<Edge2D, int>(new Edge2D.EquivalentComparator());
    private HashSet<Triangle2D> badTriangles = new HashSet<Triangle2D>();

    private bool checkForHoles;

    public BowyerWatsonFill(ICollection<Edge2D> perimiter, bool checkForHoles) {
        this.perimiter = perimiter;
        this.checkForHoles = checkForHoles;

        foreach (Edge2D edge in this.perimiter) {
            min.x = Mathf.Min(min.x, edge.a.x, edge.b.x);
            min.y = Mathf.Min(min.y, edge.a.y, edge.b.y);
            max.x = Mathf.Max(max.x, edge.a.x, edge.b.x);
            max.y = Mathf.Max(max.y, edge.a.y, edge.b.y);
        }
    }

    public void Fill() {    
        Triangle2D superTriangle = findSuperTriangle(min - new Vector2(1, 1), max + new Vector2(1, 1));
        triangles.Add(superTriangle);

        foreach (Edge2D point in perimiter) {
            processPoint(point.a);
            processPoint(point.b);
        }        

        // Cleanup        
        int intersections;
        badTriangles.Clear();
        foreach (Triangle2D tri in triangles) {
            // If the triangle has a corner of the super triangle, remove it
            if (tri.HasPoint(superTriangle.a) || tri.HasPoint(superTriangle.b) || 
                    tri.HasPoint(superTriangle.c)) {
                badTriangles.Add(tri);
                continue;
            }

            // For an arbitrary line from the centre of the triangle, 
            // see how many times we cross the perimiter. 
            // If this is even, we are outside the perimiter.      
            if (checkForHoles) {
                intersections = 0;
                Point2D centroid = tri.CentroidPoint();

                foreach (Edge2D edge in perimiter) {
                    if (edge.Intersects(centroid, superTriangle.a)) {
                        ++intersections;
                    }
                }
                if (intersections % 2 == 0) {
                    badTriangles.Add(tri);
                }

            }
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
            if (tri.HasPoint(point)) {
                // Don't need to process this triangle further
                return;
            }
            if (tri.CircumcircleContains(point)) {
                badTriangles.Add(tri);
            }
        }

        polygon.Clear();
        foreach (Triangle2D tri in badTriangles) {
            if (!triangles.Remove(tri)) {
                Debug.Log("Didn't remove triangle!: " + tri);
            }

            Edge2D ab = new Edge2D(tri.a, tri.b);
            if (polygon.ContainsKey(ab)) {
                polygon[ab] = 2;
            } else {
                polygon[ab] = 1;
            }

            Edge2D bc = new Edge2D(tri.b, tri.c);
            if (polygon.ContainsKey(bc)) {
                polygon[bc] = 2;
            } else {
                polygon[bc] = 1;
            }

            Edge2D ca = new Edge2D(tri.c, tri.a);
            if (polygon.ContainsKey(ca)) {
                polygon[ca] = 2;
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