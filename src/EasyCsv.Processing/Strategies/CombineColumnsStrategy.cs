﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EasyCsv.Components.Processing;
using EasyCsv.Core;
using EasyCsv.Core.Extensions;

namespace EasyCsv.Processing.Strategies;
public class CombineColumnsStrategy : IFullCsvProcessor
{
    public bool OperatesOnlyOnFilteredRows => true;
    private readonly Func<string?[], string?> _combineValuesFunc;
    private readonly Func<double, Task>? _onProgress;
    private readonly string[] _columnsToJoin;
    private readonly bool _combineIfNotAllPresent;
    private readonly bool _removeJoinedColumns;
    private readonly string _newColumnName;

    public CombineColumnsStrategy(string[] columnsToJoin, string newColumnName, bool combineIfNotAllPresent, bool removeJoinedColumns, Func<string?[], string?> combineValues, Func<double, Task>? onProgress = null)
    {
        if (columnsToJoin.Length == 0) throw new ArgumentException("Columns to join must be specified.", nameof(columnsToJoin));
        if (columnsToJoin.Any(string.IsNullOrWhiteSpace)) throw new ArgumentException("One of columns to join was null or whitespace.", nameof(columnsToJoin));
        if (string.IsNullOrWhiteSpace(newColumnName)) throw new ArgumentException("New column name must be specified", nameof(newColumnName));
        if (combineValues == null!) throw new ArgumentException("Combine values func can't be null", nameof(combineValues));
        _columnsToJoin = columnsToJoin;
        _combineIfNotAllPresent = combineIfNotAllPresent;
        _removeJoinedColumns = removeJoinedColumns;
        _newColumnName = newColumnName;
        _combineValuesFunc = combineValues;
        _onProgress = onProgress;
    }

    public async ValueTask<OperationResult> ProcessCsv(IEasyCsv csv, ICollection<int>? filteredRowIndexes = null)
    {
        var toCombine = _columnsToJoin.Where(x => csv.ContainsColumn(x)).ToArray();
        if (toCombine.Length == _columnsToJoin.Length || (_combineIfNotAllPresent && toCombine.Length > 0))
        {
            int total = filteredRowIndexes?.Count ?? csv.CsvContent.Count;
            int processed = 0;
            await csv.MutateAsync(async x =>
            {
                StringBuilder sb = new StringBuilder();
                foreach (var row in x.CsvContent.FilterByIndexes(filteredRowIndexes))
                {
                    string?[] values = new string[toCombine.Length];
                    int i = 0;
                    foreach (var column in toCombine)
                    {
                        values[i] = row[column]?.ToString();
                        i++;
                    }

                    row[_newColumnName] = _combineValuesFunc(values);
                    sb.Clear();
                    if (_onProgress != null)
                    {
                        processed++;
                        if (_removeJoinedColumns)
                        {
                            await _onProgress(((double)processed / total) * 0.9);
                        }
                        else
                        {
                            await _onProgress((double)processed / total);
                        }
                    }
                }

                if (_removeJoinedColumns)
                {
                    var columnsToRemove = toCombine.Where(y => y != _newColumnName);
                    x.RemoveColumns(columnsToRemove);
                    if (_onProgress != null)
                    {
                        await _onProgress(1);
                    }
                }
            }, saveChanges: false);
            return new OperationResult(false, $"Joined: {string.Join(", ", toCombine)}");
        }
        return new OperationResult(false, $"Columns to join were not present in csv. To join: {string.Join(", ", _columnsToJoin)}");
    }
}