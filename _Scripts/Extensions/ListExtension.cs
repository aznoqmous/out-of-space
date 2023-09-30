using System.Collections.Generic;
using UnityEngine;

public static class ListExtension
{
    public static T PickRandom<T>(this List<T> list)
    {
        return list.Count > 0 ? list[Random.Range(0, list.Count)] : default(T);
    }

    private static System.Random rng = new System.Random();

    public static void Shuffle<T>(this List<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }
}