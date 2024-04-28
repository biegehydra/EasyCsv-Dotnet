using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EasyCsv.Core;

namespace EasyCsv.Processing;
public class JoinColumnsStrategy : ICsvProcessor
{
    private readonly string[] _columnsToJoin;
    private readonly string _delimiter;
    private readonly bool _joinIfNotAllPresent;
    private readonly bool _removeJoinedColumns;
    private readonly string _newColumnName;

    public JoinColumnsStrategy(string[] columnsToJoin, string newColumnName, string delimiter, bool removeJoinedColumns,  bool joinIfNotAllPresent = false)
    {
        if (columnsToJoin.Length == 0) throw new ArgumentException("Columns to join must be specified.");
        if (string.IsNullOrWhiteSpace(newColumnName)) throw new ArgumentException("New column name must be specified");
        if (string.IsNullOrWhiteSpace(delimiter)) throw new ArgumentException("Delimiter name must be specified");
        _columnsToJoin = columnsToJoin;
        _delimiter = delimiter;
        _joinIfNotAllPresent = joinIfNotAllPresent;
        _removeJoinedColumns = removeJoinedColumns;
        _newColumnName = newColumnName;
    }

    public async Task<OperationResult> ProcessCsv(IEasyCsv csv)
    {
        var toJoin = _columnsToJoin.Where(x => csv.ContainsColumn(x)).ToArray();
        if (toJoin.Length == _columnsToJoin.Length || (_joinIfNotAllPresent && toJoin.Length > 0))
        {
            await csv.MutateAsync(x =>
            {
                StringBuilder sb = new StringBuilder();
                foreach (var row in x.CsvContent)
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
                }

                if (_removeJoinedColumns)
                {
                    var columnsToRemove = toJoin.Where(y => y != _newColumnName);
                    x.RemoveColumns(columnsToRemove);
                }
            });
            return new OperationResult(true, $"Joined: {string.Join(", ", toJoin)}");
        }
        return new OperationResult(false, $"Columns to join were not present in csv. To join: {string.Join(", ", _columnsToJoin)}");
    }
}