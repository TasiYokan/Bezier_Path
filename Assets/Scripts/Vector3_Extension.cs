using UnityEngine;

public static class Vector3_Extension
{
    public static Vector3 Reciprocal(this Vector3 _vec)
    {
        return new Vector3(1f / _vec.x, 1f / _vec.y, 1f / _vec.z);
    }
}