using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("EasyCsv.Processing")]
[assembly: InternalsVisibleTo("EasyCsv.Components")]
namespace EasyCsv.Core.Extensions;
internal static class Extensions
{
    internal static int IndexOf<T>(this IEnumerable<T> enumerable, T value, IEqualityComparer<T>? equalityComparer = null)
    {
        equalityComparer ??= EqualityComparer<T>.Default;
        int i = 0;
        foreach (var item in enumerable)
        {
            if (equalityComparer.Equals(item, value))
            {
                return i;
            }
            i++;
        }
        return -1;
    }
    internal static int IndexOf<T>(this IEnumerable<T> enumerable, Func<T, bool> predicate, IEqualityComparer<T>? equalityComparer = null)
    {
        equalityComparer ??= EqualityComparer<T>.Default;
        int i = 0;
        foreach (var item in enumerable)
        {
            if (predicate(item))
            {
                return i;
            }
            i++;
        }
        return -1;
    }
    internal static HashSet<T> ToHashSet<T>(this IEnumerable<T>? enumerable, IEqualityComparer<T>? equalityComparer = null)
    {
        equalityComparer ??= EqualityComparer<T>.Default;
        var hashSet = new HashSet<T>(equalityComparer);
        if (enumerable == null) return hashSet;
        foreach (var item in enumerable)
        {
            hashSet.Add(item);
        }
        return hashSet;
    }
}