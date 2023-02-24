using System;
using System.Collections.Generic;

public static class GenericExtensions
{
    public static void ForEach<T>(this List<T> list, Action<T> action)
    {
        foreach (var item in list)
        {
            action.Invoke(item);
        }
    }
    
    public static IEnumerable<TSource> DistinctBy<TSource, TKey>
        (this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
    {
        HashSet<TKey> seenKeys = new HashSet<TKey>();
        foreach (TSource element in source)
        {
            if (seenKeys.Add(keySelector(element)))
            {
                yield return element;
            }
        }
    }
    
    public static void ShiftLeft<T>(this List<T> list, int shifts)
    {
        for (int i = shifts; i < list.Count; i++)
        {
            list[i - shifts] = list[i];
        }

        for (int i = list.Count - shifts; i < list.Count; i++)
        {
            list[i] = default(T);
        }
    }
}
