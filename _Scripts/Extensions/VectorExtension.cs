using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using Unity.VisualScripting.Dependencies.NCalc;
using UnityEngine;

public static class VectorExtension
{
    public static Vector2 Rotate(this Vector2 v, float degrees)
    {
        float sin = Mathf.Sin(degrees * Mathf.Deg2Rad);
        float cos = Mathf.Cos(degrees * Mathf.Deg2Rad);

        float tx = v.x;
        float ty = v.y;
        v.x = (cos * tx) - (sin * ty);
        v.y = (sin * tx) + (cos * ty);
        return v;
    }

    public static Vector2 RotateRandom(this Vector2 v)
    {
        return v.Rotate(Random.value * 360);
    }

    public static float DistanceTo(this Vector2 v, Vector2 vto)
    {
        return (v - vto).sqrMagnitude;
    }

    public static float DistanceTo(this Vector3 v, Vector3 vto)
    {
        return (v - vto).sqrMagnitude;
    }

    public static float Angle(this Vector2 v, Vector2 vto)
    {
        Vector2 distance = vto - v;
        return Mathf.Atan2(distance.y, distance.x);
    }

    public static float Angle(this Vector3 v, Vector3 vto)
    {
        Vector3 distance = vto - v;
        return Mathf.Atan2(distance.y, distance.x);
    }

    public static Vector2 Substract(this Vector2 v, Vector2 vto)
    {
        return v - vto;
    }

    public static Vector3 Substract(this Vector3 v, Vector3 vto)
    {
        return v - vto;
    }
}
