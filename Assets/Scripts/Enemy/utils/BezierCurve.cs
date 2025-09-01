using UnityEngine;

public static class BezierCurve
{
    public static Vector2 Quadratic(Vector2 a, Vector2 b, Vector2 c, float t)
    {
        Vector2 ab = Vector2.Lerp(a, b, t);
        Vector2 bc = Vector2.Lerp(b, c, t);
        return Vector2.Lerp(ab, bc, t);
    }
    public static Vector2 Cubic(Vector2 a, Vector2 b, Vector2 c, Vector2 d, float t)
    {
        Vector2 ab = Vector2.Lerp(a, b, t);
        Vector2 bc = Vector2.Lerp(b, c, t);
        Vector2 cd = Vector2.Lerp(c, d, t);

        Vector2 abc = Vector2.Lerp(ab, bc, t);
        Vector2 bcd = Vector2.Lerp(bc, cd, t);

        return Vector2.Lerp(abc, bcd, t);
    }
}

