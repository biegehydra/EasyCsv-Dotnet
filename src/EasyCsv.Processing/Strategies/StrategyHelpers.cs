using System.Collections.Generic;

namespace EasyCsv.Processing.Strategies;

internal class StrategyHelpers
{
    internal static Dictionary<string, List<int>> MapArray(string?[] array, IEqualityComparer<string> comparer)
    {
        var map = new Dictionary<string, List<int>>(comparer);
        for (int i = 0; i < array.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(array[i])) continue;
            if (map.TryGetValue(array[i]!, out var list))
            {
                list.Add(i);
                continue;
            }
            map[array[i]!] = [i];
        }
        return map;
    }
}