using UnityEngine;

public static class VectorHelper
{
    public static Vector2Int Round(this Vector2 vector)
    {
        return new(
            Mathf.RoundToInt(vector.x),
            Mathf.RoundToInt(vector.y)
        );
    }

    public static Vector2Int Floor(this Vector2 vector)
    {
        return new(
            Mathf.FloorToInt(vector.x),
            Mathf.FloorToInt(vector.y)
        );
    }

    public static Vector2 Min(this Vector2 vector, Vector2 other)
    {
        return new(
            Mathf.Min(vector.x, other.x),
            Mathf.Min(vector.y, other.y)
        );
    }

    public static Vector2 Max(this Vector2 vector, Vector2 other)
    {
        return new(
            Mathf.Max(vector.x, other.x),
            Mathf.Max(vector.y, other.y)
        );
    }

    public static Vector2Int Min(this Vector2Int vector, Vector2Int other)
    {
        return new(
            Mathf.Min(vector.x, other.x),
            Mathf.Min(vector.y, other.y)
        );
    }

    public static Vector2Int Max(this Vector2Int vector, Vector2Int other)
    {
        return new(
            Mathf.Max(vector.x, other.x),
            Mathf.Max(vector.y, other.y)
        );
    }
}