using CsvHelper.Configuration;
using CsvHelper;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using CsvHelper.TypeConversion;
using EasyCsv.Core.Configuration;

namespace EasyCsv.Core
{
    public interface IInternalOperations
    {
        internal IEasyCsv ReplaceHeaderRow(List<string> newHeaderFields);
        internal IEasyCsv ReplaceColumn(string oldHeaderField, string newHeaderField);
        internal IEasyCsv MoveColumn(int oldIndex, int newIndex);
        internal IEasyCsv MoveColumn(string columnName, int newIndex);
        internal IEasyCsv InsertColumn(int index, string columnName, object? defaultValue);
        internal IEasyCsv AddColumn(string columnName, object? defaultValue, bool upsert = true);
        internal IEasyCsv AddColumns(IDictionary<string, object?> defaultValues, bool? upsert = true);
        internal IEasyCsv FilterRows(Func<CsvRow, bool> predicate);
        internal IEasyCsv MapValuesInColumn(string headerField, IDictionary<object, object> valueMapping);
        internal IEasyCsv SortCsv(string headerField, bool ascending = true);
        internal IEasyCsv SortCsv<TKey>(Func<CsvRow, TKey> keySelector, bool ascending = true);
        internal IEasyCsv RemoveColumn(string headerField, bool throwIfNotExists = true);
        internal IEasyCsv RemoveColumns(IEnumerable<string> columns, bool throwIfNotExists = true);
        internal IEasyCsv RemoveUnusedHeaders();
        internal Task<IEasyCsv> RemoveUnusedHeadersAsync<T>(bool caseInsensitive = true, CsvContextProfile? csvContextProfile = null);
        internal Task<IEasyCsv> RemoveUnusedHeadersAsync<T>(PrepareHeaderForMatch prepareHeaderForMatch, CsvContextProfile? csvContextProfile = null);
        internal Task<IEasyCsv> RemoveUnusedHeadersAsync<T>(CsvConfiguration csvConfig, CsvContextProfile? csvContextProfile = null);
        internal IEasyCsv SwapColumns(int columnOneIndex, int columnTwoIndex);
        internal IEasyCsv SwapColumns(string columnOneName, string columnTwoName, IEqualityComparer<string>? comparer = null);
    }
}