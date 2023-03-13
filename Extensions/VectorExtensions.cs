using UnityEngine;

public static class VectorExtensions
{
    public static UnityEngine.Vector2 WithX(this Vector2 vector , float x)
    {
        return new Vector2(x, vector.y);
    }
    
    public static UnityEngine.Vector2 WithAddX(this Vector2 vector , float x)
    {
        return new Vector2(vector.x + x, vector.y);
    }
    
    public static UnityEngine.Vector3 WithX(this Vector3 vector , float x)
    {
        return new Vector3(x, vector.y, vector.z);
    }
    
    public static UnityEngine.Vector3 WithAddX(this Vector3 vector , float x)
    {
        return new Vector3(vector.x + x, vector.y, vector.z);
    }
}
