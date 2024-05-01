using System;
using System.Threading.Tasks;

namespace EasyCsv.Processing.Strategies
{
    public class ShouldDeleteColumnStrategy : ICsvColumnDeleteEvaluator
    {
        public string ColumnName { get; }
        private Func<ICell, bool> _shouldDeleteFunc;
        public ShouldDeleteColumnStrategy(string columnName, Func<ICell, bool> shouldDeleteFunc)
        {
            ColumnName = columnName;
            _shouldDeleteFunc = shouldDeleteFunc;
        }

        public ValueTask<OperationDeleteResult> EvaluateDelete<TCell>(TCell cell) where TCell : ICell
        {
            if (_shouldDeleteFunc(cell))
            {
                return new ValueTask<OperationDeleteResult>(new OperationDeleteResult(true, false));
            }
            return new ValueTask<OperationDeleteResult>(new OperationDeleteResult(true, false));
        }
    }
}
