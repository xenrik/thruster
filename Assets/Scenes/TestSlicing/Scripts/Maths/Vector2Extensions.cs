using UnityEngine;
using System;

public static class Vector2Extensions {
    public static Vector2 Round(this Vector2 v, int decimals) {
        return new Vector2((float)Math.Round(v.x, decimals), (float)Math.Round(v.y, decimals));
    }
}