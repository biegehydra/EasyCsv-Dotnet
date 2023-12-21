using System.Collections.Generic;
using System.Dynamic;

namespace EasyCsv.Core
{
    public class CsvRow : DynamicObject
    {
        private readonly IDictionary<string, object?> _innerDictionary;

        internal CsvRow(IEnumerable<string> headers)
        {
            _innerDictionary = new Dictionary<string, object?>();
            foreach (var header in headers)
            {
                this[header] = null;
            }
        }

        public CsvRow(CsvRow csvRow)
        {
            _innerDictionary = new Dictionary<string, object?>(csvRow._innerDictionary);
        }

        public CsvRow(IDictionary<string, object?> row)
        {
            _innerDictionary = new Dictionary<string, object?>(row);
        }

        public CsvRow(IEnumerable<string> headers, List<string> values)
        {
            _innerDictionary = new Dictionary<string, object?>();
            var i = 0;
            foreach (var key in headers)
            {
                this[key] = values[i];
                i++;
            }
        }

        public object? this[string key]
        {
            get => _innerDictionary[key];
            set => _innerDictionary[key] = value;
        }

        public int Count => _innerDictionary.Count;

        public ICollection<string> Keys => _innerDictionary.Keys;

        public ICollection<object?> Values => _innerDictionary.Values;

        public void Add(string key, object? value)
        {
            _innerDictionary.Add(key, value);
        }

        public bool ContainsKey(string key)
        {
            return _innerDictionary.ContainsKey(key);
        }

        public bool Remove(string key)
        {
            return _innerDictionary.Remove(key);
        }

        public bool TryGetValue(string key, out object? value)
        {
            return _innerDictionary.TryGetValue(key, out value);
        }

        public void Add(KeyValuePair<string, object?> item)
        {
            _innerDictionary.Add(item.Key, item.Value);
        }

        public void Clear()
        {
            _innerDictionary.Clear();
        }

        public bool Contains(KeyValuePair<string, object?> item)
        {
            return _innerDictionary.Contains(item);
        }

        public void CopyTo(KeyValuePair<string, object?>[] array, int arrayIndex)
        {
            _innerDictionary.CopyTo(array, arrayIndex);
        }

        public bool Remove(KeyValuePair<string, object?> item)
        {
            return _innerDictionary.Remove(item.Key);
        }

        public IEnumerator<KeyValuePair<string, object?>> GetEnumerator()
        {
            return _innerDictionary.GetEnumerator();
        }

        public override bool TryGetMember(GetMemberBinder binder, out object? result)
        {
            return _innerDictionary.TryGetValue(binder.Name, out result);
        }

        public override bool TrySetMember(SetMemberBinder binder, object? value)
        {
            _innerDictionary[binder.Name] = value;
            return true;
        }

        public override IEnumerable<string> GetDynamicMemberNames()
        {
            return Keys;
        }

        public Dictionary<string, object?> ToDictionary()
        {
            return new Dictionary<string, object?>(_innerDictionary);
        }
    }
}