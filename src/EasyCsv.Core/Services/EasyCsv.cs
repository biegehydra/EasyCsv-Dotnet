using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using EasyCsv.Core.Configuration;

[assembly: InternalsVisibleTo("EasyCsv.Files")]
[assembly: InternalsVisibleTo("EasyCsv.Tests.Files")]
[assembly: InternalsVisibleTo("EasyCsv.Tests.Core")]
[assembly: InternalsVisibleTo("EasyCsv.Tests.Processing")]
namespace EasyCsv.Core
{
    internal partial class EasyCsv : IEasyCsv
    {
        private EasyCsvConfiguration Config { get; }
        public byte[]? ContentBytes { get; internal set; }
        public string? ContentStr { get; internal set; }

        public List<CsvRow>? CsvContent { get; set; }

        public int RowCount() => CsvContent?.Count ?? 0;

        public bool ContainsColumn(string column, bool caseInsensitive = false) => ColumnNames()?.Contains(column, caseInsensitive ? StringComparer.CurrentCultureIgnoreCase : StringComparer.Ordinal) ?? false;

        public bool ContainsAllColumns(IEnumerable<string> columns, IEqualityComparer<string>? comparer = null)
        {
            comparer ??= StringComparer.Ordinal; 
            var headers = ColumnNames();
            if (headers == null) return false;
            foreach (var column in columns)
            {
                if (!headers.Contains(column, comparer))
                {
                    return false;
                }
            }
            return true;
        }

        public bool ContainsRow(CsvRow row)
        {
            return CsvContent?.Any(r => RowsEqual(r, row)) ?? false;
        }

        public void Mutate(Action<CSVMutationScope> mutations, bool saveChanges = true)
        {
            var scope = new CSVMutationScope(this);
            mutations(scope);
            if (saveChanges)
            {
                CalculateContentBytesAndStr();
            }
        }

        public async Task MutateAsync(Func<CSVMutationScope, Task> mutations, bool saveChanges = true)
        {
            var scope = new CSVMutationScope(this);
            await mutations(scope);
            if (saveChanges)
            {
                await CalculateContentBytesAndStrAsync();
            }
        }

        public async Task MutateAsync(Action<CSVMutationScope> mutations, bool saveChanges = true)
        {
            var scope = new CSVMutationScope(this);
            mutations(scope);
            if (saveChanges)
            {
                await CalculateContentBytesAndStrAsync();
            }
        }

        private static bool RowsEqual(CsvRow row1, CsvRow row2)
        {
            return row1.Count == row2.Count && row1.Keys.All(key => row2.ContainsKey(key) && Equals(row1[key], row2[key]));
        }

        public string[]? ColumnNames()
        {
            return CsvContent?.FirstOrDefault()?.Keys.ToArray();
        }

        private static List<CsvRow> CloneContent(List<CsvRow>? content)
        {
            return content?.Select(row => new CsvRow(row)).ToList() ?? new List<CsvRow>();
        }

        public IEasyCsv Clone()
        {
            return new EasyCsv(CloneContent(CsvContent), Config);
        }
    }
}
