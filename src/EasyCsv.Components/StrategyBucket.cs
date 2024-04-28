namespace EasyCsv.Components
{
    internal class StrategyBucket
    {
        public string ColumnName { get; set; }
        private readonly HashSet<StrategyItem> _strategies = new();

        public StrategyBucket(string columnName)
        {
            ColumnName = columnName;
        }

        public IEnumerable<StrategyItem> Search(string query)
        {
            return _strategies.Where(x => x.DisplayName?.Contains(query) == true || x.Description?.Contains(query) == true);
        }

        public void Add(StrategyItem strategy)
        {
            if (strategy != null!)
            {
                _strategies.Add(strategy);
            }
        }

        public void Remove(StrategyItem strategy)
        {
            if (strategy != null!)
            {
                _strategies.Remove(strategy);
            }
        }
    }
}
