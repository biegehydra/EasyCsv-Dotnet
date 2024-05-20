using EasyCsv.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
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

        internal CsvRow(Dictionary<string, object?> row, bool useAsInner)
        {
            _innerDictionary = row;
        }

        public CsvRow(CsvRow csvRow)
        {
            _innerDictionary = new Dictionary<string, object?>(csvRow._innerDictionary);
        }

        public CsvRow(IDictionary<string, object?> row)
        {
            _innerDictionary = new Dictionary<string, object?>(row);
        }

        public CsvRow(IEnumerable<string> columnNames, List<object?> values)
        {
            _innerDictionary = new Dictionary<string, object?>();
            var i = 0;
            foreach (var columnName in columnNames)
            {
                this[columnName] = values[i];
                i++;
            }
        }

        public object? this[string columnName]
        {
            get => _innerDictionary[columnName];
            set => _innerDictionary[columnName] = value;
        }

        public int Count => _innerDictionary.Count;

        public ICollection<string> Keys => _innerDictionary.Keys;

        public ICollection<object?> Values => _innerDictionary.Values;

        public void Add(string columnName, object? value)
        {
            _innerDictionary.Add(columnName, value);
        }

        public bool ContainsKey(string columnName)
        {
            return _innerDictionary.ContainsKey(columnName);
        }

        public bool Remove(string columnName)
        {
            return _innerDictionary.Remove(columnName);
        }

        public bool TryGetValue(string columnName, out object? value)
        {
            return _innerDictionary.TryGetValue(columnName, out value);
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

        internal void SetValueAtIndex(int index, object? value)
        {
            var key = KeyAt(index);
            _innerDictionary[key] = value;
        }

        internal (string, object?) KvpAt(int index)
        {
            return (KeyAt(index), ValueAt(index));
        }

        internal string KeyAt(int index)
        {
            return _innerDictionary.Keys.ElementAt(index);
        }


        internal object? ValueAt(int index)
        {
            return _innerDictionary.Values.ElementAt(index);
        }

        public bool AnyColumnMatchesValues(IEnumerable<string>? columns, object? value, IEqualityComparer<object>? comparer = null)
        {
            if (value == null || columns == null!) return true;
            comparer ??= EqualityComparer<object>.Default;
            foreach (var column in columns)
            {
                if (TryGetValue(column, out object? columnValue) && comparer.Equals(columnValue, value))
                {
                    return true;
                }
            }
            return false;
        }

        public bool AnyColumnContainsValues(IEnumerable<string>? columns, string? value, StringComparison stringComparisonType = StringComparison.OrdinalIgnoreCase)
        {
            if (string.IsNullOrWhiteSpace(value) || columns == null) return true;
            foreach (var column in columns)
            {
#if NETSTANDARD2_0
                if (TryGetValue(column, out object? columnValue) && columnValue?.ToString()?.Contains(value) == true)
#else
                if (TryGetValue(column, out object? columnValue) && columnValue?.ToString()?.Contains(value, stringComparisonType) == true)
#endif
                {
                    return true;
                }
            }
            return false;
        }

        public bool RemoveProcessingReference(int referenceCsvId, int referenceRowId)
        {
            var processingReferences = ProcessingReferences();
            if (processingReferences?.Length is not > 0) return false;
            var referenceIndex = processingReferences.IndexOf(x => x.ReferenceCsvIndex == referenceCsvId && x.ReferenceRowIndex == referenceRowId);
            if (referenceIndex >= 0)
            {
                string newReferencesStr = string.Join(",", processingReferences.TakeSkipTakeRest(referenceIndex, 1).Select(x => $"{x.ReferenceCsvIndex}-{x.ReferenceRowIndex}"));
                _innerDictionary[InternalColumnNames.References] = newReferencesStr;
            }
            return false;
        }

        public bool RemoveProcessingTag(string tag)
        {
            var tags = ProcessingTags();
            if (tags?.Length is not > 0) return false;
            int tagIndex = tags.IndexOf(tag);
            if (tagIndex >= 0)
            {
                string newTagsStr = string.Join(",", tags.TakeSkipTakeRest(tagIndex, 1));
                _innerDictionary[InternalColumnNames.Tags] = newTagsStr;
                return true;
            }
            return false;
        }

        public bool ContainsProcessingTag(string tag)
        {
            if (string.IsNullOrWhiteSpace(tag)) return false;
            var existingTags = ProcessingTags();
            if (existingTags == null) return false;
            return existingTags.Contains(tag);
        }

        public bool ContainsAnyProcessingTag(IEnumerable<string> tags)
        {
            if (tags == null!) return false;
            var existingTags = ProcessingTags();
            if (existingTags == null) return false;
            foreach (var tag in tags)
            {
                if (string.IsNullOrWhiteSpace(tag)) continue;
                if (existingTags.Contains(tag)) return true;
            }
            return false;
        }

        public bool MatchesIncludeTagsAndExcludeTags(ICollection<string>? includeTags, ICollection<string>? excludeTags)
        {
            if (includeTags == null && excludeTags == null) return true;
            if (includeTags?.Count == 0 && excludeTags?.Count == 0) return true;
            var tags = ProcessingTags()?.ToHashSet();
            if (includeTags?.Count > 0 && (tags == null || tags.Count == 0)) return false;
            if (excludeTags != null && tags != null)
            {
                foreach (var excludeTag in excludeTags)
                {
                    if (string.IsNullOrWhiteSpace(excludeTag)) continue;
                    if (tags.Contains(excludeTag) == true) return false;
                }
            }
            bool any = false;
            if (includeTags != null && tags != null)
            {
                foreach (var includeTag in includeTags)
                {
                    if (string.IsNullOrWhiteSpace(includeTag)) continue;
                    any = true;
                    if (tags.Contains(includeTag)) return true;
                }
            }
            return !any;
        }

        public string[]? ProcessingTags()
        {
            if (!_innerDictionary.TryGetValue(InternalColumnNames.Tags, out var value))
            {
                return null;
            }

            var valueStr = value?.ToString();
            if (string.IsNullOrWhiteSpace(valueStr)) return null;
#if NETSTANDARD2_0
            var split = valueStr!.Split([","], StringSplitOptions.RemoveEmptyEntries);
#else
            var split = valueStr!.Split(',', StringSplitOptions.RemoveEmptyEntries);
#endif
            return split;
        }

        public ProcessingReference[]? ProcessingReferences()
        {
            if (!_innerDictionary.TryGetValue(InternalColumnNames.References, out var value))
            {
                return null;
            }
            var valueStr = value?.ToString();
            if (string.IsNullOrWhiteSpace(valueStr)) return null;
#if NETSTANDARD2_0
            var split = valueStr!.Split([","], StringSplitOptions.RemoveEmptyEntries);
#else
            var split = valueStr!.Split(',', StringSplitOptions.RemoveEmptyEntries);
#endif
            return split.Select(ParseIntegers).ToArray();
        }

        private static ProcessingReference ParseIntegers(string input)
        {
            int dashIndex = input.IndexOf('-');
            if (dashIndex == -1)
            {
                throw new ArgumentException("Input must contain a dash.");
            }

#if NETSTANDARD2_0
            string leftSpan = input.Substring(0, dashIndex);
            string rightSpan = input.Substring(dashIndex + 1);
#else
            ReadOnlySpan<char> leftSpan = input.AsSpan(0, dashIndex);
            ReadOnlySpan<char> rightSpan = input.AsSpan(dashIndex + 1);
#endif

            if (int.TryParse(leftSpan, out int left) && int.TryParse(rightSpan, out int right))
            {
                return new ProcessingReference(left, right);
            }
            else
            {
                throw new ArgumentException("Both parts of the input must be valid integers.");
            }
        }

        public Dictionary<string, object?> ToDictionary()
        {
            return new Dictionary<string, object?>(_innerDictionary);
        }

        public Dictionary<string, string?> ToStringDictionary()
        {
            return _innerDictionary.ToDictionary(x => x.Key, x => x.Value?.ToString());
        }

        public CsvRow Clone()
        {
            return new CsvRow(_innerDictionary);
        }
        public void AddProcessingTag(string tag)
        {
            var existingTags = ProcessingTags()?.ToHashSet() ?? new HashSet<string>();
            bool added = existingTags.Add(tag);
            if (added)
            {
                this[InternalColumnNames.Tags] = string.Join(",", existingTags);
            }
        }

        public void AddProcessingReference(int referenceCsvId, int referenceRowIndex)
        {
            var existingReferences = ProcessingReferences()?.ToHashSet() ?? new HashSet<ProcessingReference>();
            if (existingReferences.Add(new ProcessingReference(referenceCsvId, referenceRowIndex)))
            {
                this[InternalColumnNames.References] = string.Join(",", existingReferences);
            }
        }

        public void AddProcessingTags(IEnumerable<string> tags)
        {
            var existingTags = Extensions.Extensions.ToHashSet(ProcessingTags());
            foreach (var tag in tags)
            {
                existingTags.Add(tag);
            }
            this[InternalColumnNames.Tags] = string.Join(",", existingTags);
        }

        public void AddProcessingReferences(int referenceCsvId, IEnumerable<int> referenceRowIndexes)
        {
            var newReferences = referenceRowIndexes.Select(x => new ProcessingReference(referenceCsvId, x)).ToHashSet();
            var existingReferences = ProcessingReferences();
            if (existingReferences != null)
            {
                foreach (var existingReference in existingReferences)
                {
                    newReferences.Add(existingReference);
                }
            }
            this[InternalColumnNames.References] = string.Join(",", newReferences);
        }

        public bool ValuesEqual(CsvRow other)
        {
            return Count == other.Count && Keys.All(key => ContainsKey(key) && Equals(this[key], other[key]));
        }

        public void MapValuesTo(CsvRow other)
        {
            if (other?._innerDictionary == null!) return;
            foreach (var kvp in _innerDictionary)
            {
                if (other._innerDictionary.ContainsKey(kvp.Key))
                {
                    other._innerDictionary[kvp.Key] = kvp.Value;
                }
            }
        }
    }
}

public readonly record struct ProcessingReference
{
    public ProcessingReference(int referenceCsvIndex, int referenceRowIndex)
    {
        ReferenceCsvIndex = referenceCsvIndex;
        ReferenceRowIndex = referenceRowIndex;
    }

    public int ReferenceCsvIndex { get; }
    public int ReferenceRowIndex { get; }
}