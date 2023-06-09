﻿using CsvHelper.Configuration;
using CsvHelper;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

namespace EasyCsv.Core
{
    public interface IInternalOperations
    {
        internal IEasyCsv ReplaceHeaderRow(List<string> newHeaderFields);
        internal IEasyCsv ReplaceColumn(string oldHeaderField, string newHeaderField);
        internal IEasyCsv AddColumn(string header, string value, bool upsert = true);
        internal IEasyCsv AddColumns(IDictionary<string, string> defaultValues, bool upsert = true);
        internal IEasyCsv FilterRows(Func<CsvRow, bool> predicate);
        internal IEasyCsv MapValuesInColumn(string headerField, IDictionary<object, object> valueMapping);
        internal IEasyCsv SortCsv(string headerField, bool ascending = true);
        internal IEasyCsv SortCsv<TKey>(Func<CsvRow, TKey> keySelector, bool ascending = true);
        internal IEasyCsv RemoveColumn(string headerField);
        internal IEasyCsv RemoveColumns(List<string> headerFields);
        internal Task<IEasyCsv> RemoveUnusedHeadersAsync<T>(bool caseInsensitive = true);
        internal Task<IEasyCsv> RemoveUnusedHeadersAsync<T>(PrepareHeaderForMatch prepareHeaderForMatch);
        internal Task<IEasyCsv> RemoveUnusedHeadersAsync<T>(CsvConfiguration csvConfig);
    }
}