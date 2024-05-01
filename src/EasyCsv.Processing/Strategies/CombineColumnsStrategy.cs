using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EasyCsv.Core;
using EasyCsv.Core.Extensions;

namespace EasyCsv.Processing.Strategies;

public abstract class CombineColumnsStrategy : ICsvProcessor
{
    private readonly Func<string?[], string?> _combineValuesFunc;
    private readonly string[] _columnsToJoin;
    private readonly bool _combineIfNotAllPresent;
    private readonly bool _removeJoinedColumns;
    private readonly string _newColumnName;

    public CombineColumnsStrategy(string[] columnsToJoin, string newColumnName, bool combineIfNotAllPresent, bool removeJoinedColumns, Func<string?[], string?> combineValues)
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
    }

    public async ValueTask<OperationResult> ProcessCsv(IEasyCsv csv, ICollection<int>? filteredRowIndexes = null)
    {
        var toCombine = _columnsToJoin.Where(x => csv.ContainsColumn(x)).ToArray();
        if (toCombine.Length == _columnsToJoin.Length || (_combineIfNotAllPresent && toCombine.Length > 0))
        {
            await csv.MutateAsync(x =>
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
                }

                if (_removeJoinedColumns)
                {
                    var columnsToRemove = toCombine.Where(y => y != _newColumnName);
                    x.RemoveColumns(columnsToRemove);
                }
            });
            return new OperationResult(false, $"Joined: {string.Join(", ", toCombine)}");
        }
        return new OperationResult(false, $"Columns to join were not present in csv. To join: {string.Join(", ", _columnsToJoin)}");
    }
}