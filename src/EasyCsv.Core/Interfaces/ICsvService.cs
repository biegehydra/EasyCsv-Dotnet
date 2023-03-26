using System;
using System.Collections.Generic;
using System.Threading.Tasks;
namespace EasyCsv.Core
{
    public interface ICsvService
    {
        byte[]? FileContentBytes { get; set; }
        string? FileContentStr { get; set; }
        List<IDictionary<string, object>>? CsvContent { get; }
        List<string>? GetHeaders();
        void ReplaceColumn(string oldHeaderField, string newHeaderField);
        void CreateColumnWithDefaultValue(Dictionary<string, string> defaultValues, bool upsert = true);
        void CreateColumnsWithDefaultValue(string header, string value, bool upsert = true);
        void FilterRows(Func<IDictionary<string, object>, bool> predicate);
        void MapValuesInColumn(string headerField, Dictionary<object, object> valueMapping);
        void SortCsvByColumnData(string headerField, bool ascending = true);
        void SortCsv(string headerField, Func<IDictionary<string, object>, bool> predicate, bool ascending = true);
        void SortRows<TKey>(Func<IDictionary<string, object>, TKey> keySelector, bool ascending = true);
        void RemoveColumns(List<string> headerFields);
        void RemoveColumn(string headerField);
        Task CalculateFileContent();
        List<T> GetRecords<T>() where T : new();
    }
}