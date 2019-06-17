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
        ScriptProfiler.StartMethod();

        this.perimiter = perimiter;
        this.checkForHoles = checkForHoles;

        foreach (Edge2D edge in perimiter) {
            min.x = Mathf.Min(min.x, edge.a.x, edge.b.x);
            min.y = Mathf.Min(min.y, edge.a.y, edge.b.y);
            max.x = Mathf.Max(max.x, edge.a.x, edge.b.x);
            max.y = Mathf.Max(max.y, edge.a.y, edge.b.y);
        }
    
        ScriptProfiler.EndMethod();
    }

    public void Fill() {    
        ScriptProfiler.StartMethod();

        foreach (Slicer.Debug debug in Fill(null)) {
            // Do nothing
        }
        
        ScriptProfiler.EndMethod();
    }

    public IEnumerable<Slicer.Debug> Fill(Slicer.Debug debug) {        
        ScriptProfiler.StartMethod();

        Triangle2D superTriangle = findSuperTriangle(min - new Vector2(1, 1), max + new Vector2(1, 1));
        triangles.Add(superTriangle);

        ScriptProfiler.StartGroup("Fill");
        foreach (Edge2D point in perimiter) {
            processPoint(point.a, debug != null);
            if (debug != null) {
                yield return PopulateDebug(debug, point.a);
            }

            processPoint(point.b, debug != null);
            if (debug != null) {
                yield return PopulateDebug(debug, point.b);
            }
        }        
        ScriptProfiler.EndGroup();

        // Cleanup        
        ScriptProfiler.StartGroup("Cleanup");
        HashSet<Edge2D> intersections = new HashSet<Edge2D>();
        badTriangles.Clear();
        newTriangles.Clear();
        int holes = 0;
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
                newTriangles.Clear();
                intersections.Clear();

                Point2D centroid = tri.CentroidPoint();

                foreach (Edge2D edge in perimiter) {
                    if (edge.Intersects(centroid, superTriangle.a)) {
                        intersections.Add(edge);
                    }
                }
                if (intersections.Count % 2 == 0) {
                    newTriangles.Add(tri);
                    if (debug != null) {
                        Debug.Log($"Intersections for: {tri} = {intersections.Count} (remove)");
                        yield return PopulateDebug(debug, testTriangle: tri, testEdge: new Edge2D(centroid, superTriangle.a));
                    }

                    ++holes;
                    badTriangles.Add(tri);
                } else if (debug != null) {
                    Debug.Log($"Intersections for: {tri} = {intersections.Count}");
                    yield return PopulateDebug(debug, testTriangle: tri, testEdge: new Edge2D(centroid, superTriangle.a));
                }

            }
        }

        //Debug.Log($"Removed {holes} triangles that were holes");

        foreach (Triangle2D tri in badTriangles) {
            triangles.Remove(tri);
        }
        ScriptProfiler.EndGroup();
        ScriptProfiler.EndMethod();
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

    private void processPoint(Point2D point, bool debug) {
        if (debug) {
            Debug.Log("----------------");
            Debug.Log($"Processing Point: {point}");
        }

        badTriangles.Clear();
        foreach (Triangle2D tri in triangles) {
            if (tri.HasPoint(point)) {
                // Don't need to process this triangle further
                badTriangles.Clear();
                break;
            }
            if (tri.CircumcircleContains(point)) {
                badTriangles.Add(tri);

                if (debug) {
                    Debug.Log($"   Bad Triangle: {tri}");
                }
            }
        }

        polygon.Clear();
        foreach (Triangle2D tri in badTriangles) {
            if (!triangles.Remove(tri)) {
                Debug.Log("Didn't remove triangle!: " + tri);
            }

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

        newTriangles.Clear();
        foreach (KeyValuePair<Edge2D,int> edge in polygon) {
            if (edge.Value == 1) {
                Triangle2D tri = new Triangle2D(edge.Key.a, edge.Key.b, point);
                newTriangles.Add(tri);

                if (debug) {
                    Debug.Log($"   New Triangle: {tri}");
                }
            }
        }

        triangles.UnionWith(newTriangles);
    }

    private Slicer.Debug PopulateDebug(Slicer.Debug debug, Point2D? testPoint = null, Triangle2D? testTriangle = null, Edge2D? testEdge = null) {
        debug.Reset();
        if (testPoint != null) {
            debug.SetTestPoint((Point2D)testPoint);
        }
        if (testTriangle != null) {
            debug.SetTestTriangle((Triangle2D)testTriangle);
        }
        if (testEdge != null) {
            debug.SetTestEdge((Edge2D)testEdge);
        }

        foreach (Edge2D edge in perimiter) {
            debug.AddPerimiterPoint(edge.a);
            debug.AddPerimiterPoint(edge.b);
        }

        foreach (Triangle2D tri in badTriangles) {
            debug.AddBadTriangle(tri);
        }

        foreach (Triangle2D tri in newTriangles) {
            debug.AddNewTriangle(tri);
        }

        foreach (Triangle2D tri in triangles) {
            debug.AddTriangle(tri);
        }

        return debug;
    }
}