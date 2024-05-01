using System.Collections.Generic;

namespace EasyCsv.Core.Extensions;
public static class CsvExtensions
{
    public static IEnumerable<T> FilterByIndexes<T>(this IEnumerable<T>? enumerable, ICollection<int>? indexes)
    {
        if (enumerable == null) yield break;
        int i = 0;
        foreach (var item in enumerable)
        {
            if (indexes == null || indexes.Contains(i))
            {
                yield return item;
            }
            i++;
        }
    }
}
