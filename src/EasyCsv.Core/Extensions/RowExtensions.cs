using System.Collections.Generic;

namespace EasyCsv.Core.Extensions;
public static class RowExtensions
{
    public static bool ContainsAllColumns(this CsvRow? row, IEnumerable<string> columns)
    {
        if (row == null) return false;
        foreach (var column in columns)
        {
            if (!row.ContainsKey(column))
            {
                return false;
            }
        }
        return true;
    }
}