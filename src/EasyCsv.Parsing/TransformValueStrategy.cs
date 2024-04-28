﻿using System;
using System.Threading.Tasks;
namespace EasyCsv.Parsing;
public class TransformValueStrategy : ICsvColumnProcessor
{
    public string ColumnName { get; }
    private readonly Func<object?, object?> _transformValue;
    public TransformValueStrategy(string columnName, Func<object?, object?> transformValue)
    {
        ColumnName = columnName;
        _transformValue = transformValue;
    }

    public Task<OperationResult> ProcessCell(ICell cell)
    {
        cell.Value = _transformValue(cell.Value);
        return Task.FromResult(new OperationResult(true, null));
    }
}