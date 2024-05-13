using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EasyCsv.Core;
using EasyCsv.Core.Extensions;

namespace EasyCsv.Processing.Strategies;
public class JoinColumnsStrategy : IFullCsvProcessor
{
    public bool OperatesOnlyOnFilteredRows => true;
    private readonly string[] _columnsToJoin;
    private readonly string _delimiter;
    private readonly bool _joinIfNotAllPresent;
    private readonly Func<double, Task>? _onProgress;
    private readonly bool _removeJoinedColumns;
    private readonly string _newColumnName;

    public JoinColumnsStrategy(string[] columnsToJoin, string newColumnName, string delimiter, bool removeJoinedColumns,  bool joinIfNotAllPresent = false, Func<double, Task>? onProgress = null)
    {
        if (columnsToJoin.Length == 0) throw new ArgumentException("Columns to join must be specified.");
        if (string.IsNullOrWhiteSpace(newColumnName)) throw new ArgumentException("New column name must be specified");
        _columnsToJoin = columnsToJoin;
        _delimiter = delimiter ?? throw new ArgumentException("Delimiter cannot be null");
        _joinIfNotAllPresent = joinIfNotAllPresent;
        _onProgress = onProgress;
        _removeJoinedColumns = removeJoinedColumns;
        _newColumnName = newColumnName;
    }

    public async ValueTask<OperationResult> ProcessCsv(IEasyCsv csv, ICollection<int>? filteredRowIndexes = null)
    {
        var toJoin = _columnsToJoin.Where(x => csv.ContainsColumn(x)).ToArray();
        if (toJoin.Length == _columnsToJoin.Length || (_joinIfNotAllPresent && toJoin.Length > 0))
        {
            await csv.MutateAsync(async x =>
            {
                StringBuilder sb = new StringBuilder();
                int total = filteredRowIndexes?.Count ?? csv.CsvContent.Count;
                int processed = 0;
                foreach (var row in x.CsvContent.FilterByIndexes(filteredRowIndexes))
                {
                    foreach (var column in toJoin)
                    {
                        if (sb.Length == 0)
                        {
                            sb.Append(row[column]);
                        }
                        else
                        {
                            sb.Append(_delimiter);
                            sb.Append(row[column]);
                        }
                    }

                    row[_newColumnName] = sb.ToString();
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
                    var columnsToRemove = toJoin.Where(y => y != _newColumnName);
                    x.RemoveColumns(columnsToRemove);
                    if (_onProgress != null)
                    {
                        await _onProgress(1);
                    }
                }
            }, saveChanges: false);
            return new OperationResult(true, $"Joined: {string.Join(", ", toJoin)}");
        }
        return new OperationResult(false, $"Columns to join were not present in csv. To join: {string.Join(", ", _columnsToJoin)}");
    }
}