using UnityEngine;

public class MoreGizmos {

    public static void DrawLine(Vector3 p1, Vector3 p2, float width) {
        int count = 1 + Mathf.CeilToInt(width); // how many lines are needed.
        if (count == 1) {
            Gizmos.DrawLine(p1, p2);
        } else {
            Camera c = Camera.current;
            if (c == null) {
                Debug.LogError("Camera.current is null");
                return;
            }
            var scp1 = c.WorldToScreenPoint(p1);
            var scp2 = c.WorldToScreenPoint(p2);

            Vector3 v1 = (scp2 - scp1).normalized; // line direction
            Vector3 n = Vector3.Cross(v1, Vector3.forward); // normal vector

            for (int i = 0; i < count; i++) {
                Vector3 o = 0.99f * n * width * ((float)i / (count - 1) - 0.5f);
                Vector3 origin = c.ScreenToWorldPoint(scp1 + o);
                Vector3 destiny = c.ScreenToWorldPoint(scp2 + o);
                Gizmos.DrawLine(origin, destiny);
            }
        }
    }

    public static void DrawCircle(Vector3 origin, Vector3 normal, float radius, float width = 1, float resolution = 5) {
        Quaternion q = Quaternion.LookRotation(normal);
        Vector3 offset = Vector3.up * radius;

        Vector3 a = (q * offset) + origin;
        Vector3 b;        
        for (float r = resolution; r <= 360; r += resolution) {
            b = (Quaternion.Euler(0, r, 0) * q * offset) + origin;
            DrawLine(a, b, width);

            a = b;
        }
    }
}