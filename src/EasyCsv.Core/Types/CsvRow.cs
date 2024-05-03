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

        public CsvRow(IEnumerable<string> headers, List<object?> values)
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

        public bool RemoveReference(int referencesColumnIndex, int referenceCsvId, int referenceRowId)
        {
            if (!Utils.IsValidIndex(referencesColumnIndex, Count)) return false;
            string? value = ValueAt(referencesColumnIndex)?.ToString();
            if (string.IsNullOrWhiteSpace(value)) return false;
            var split = value.Split([","], StringSplitOptions.RemoveEmptyEntries);
            var parsedIntegers = split.Select(ParseIntegers).ToArray();
            var referenceIndex = parsedIntegers.IndexOf(x => x.Left == referenceCsvId && x.Right == referenceRowId);
            if (referenceIndex >= 0)
            {
                string newReferencesStr = string.Join(",", parsedIntegers.Take(referenceIndex).Skip(1).Select(x => $"{referenceCsvId}-{x}"));
                SetValueAtIndex(referencesColumnIndex, newReferencesStr);
            }
            return false;
        }

        public bool RemoveReference(int referenceCsvId, int referenceRowId)
        {
            int referenceColumnIndex = Keys.IndexOf(InternalColumnNames.References);
            return RemoveReference(referenceColumnIndex, referenceCsvId, referenceRowId);
        }


        public bool RemoveTag(int tagsColumnIndex, string tag)
        {
            if (!Utils.IsValidIndex(tagsColumnIndex, Values.Count)) return false;
            var tags = ValueAt(tagsColumnIndex)?.ToString()?.Split([","], StringSplitOptions.RemoveEmptyEntries);
            int? tagIndex = tags?.IndexOf(tag);
            if (tagIndex >= 0)
            {
                string newTagsStr = string.Join(",", tags!.Take(tagsColumnIndex).Skip(1)); 
                SetValueAtIndex(tagsColumnIndex, newTagsStr);
                return true;
            }
            return false;
        }

        public bool RemoveTag(string tag)
        {
            int tagsColumnIndex = Keys.IndexOf(InternalColumnNames.Tags);
            return RemoveTag(tagsColumnIndex, tag);
        }

        public bool ContainsTag(int tagsColumnIndex, string tag)
        {
            if (string.IsNullOrWhiteSpace(tag)) return false;
            return ValueAt(tagsColumnIndex)?.ToString()?.Contains(tag) == true;
        }

        public bool ContainsAnyTag(int tagsColumnIndex, IEnumerable<string> tags)
        {
            if (tags == null!) return false;
            string? strValue = ValueAt(tagsColumnIndex)?.ToString();
            if (string.IsNullOrWhiteSpace(strValue)) return false;
            foreach (var tag in tags)
            {
                if (string.IsNullOrWhiteSpace(tag)) continue;
                if (strValue!.Contains(tag)) return true;
            }
            return false;
        }

        public bool MatchesIncludeTagsAndExcludeTags(int tagsColumnIndex, ICollection<string>? includeTags, ICollection<string>? excludeTags)
        {
            if (includeTags == null && excludeTags == null) return true;
            if (includeTags?.Count == 0 && excludeTags?.Count == 0) return true;
            string? strValue = ValueAt(tagsColumnIndex)?.ToString();
            if (includeTags?.Count > 0 && string.IsNullOrWhiteSpace(strValue)) return false;
            if (excludeTags != null)
            {
                foreach (var excludeTag in excludeTags)
                {
                    if (string.IsNullOrWhiteSpace(excludeTag)) continue;
                    if (strValue?.Contains(excludeTag) == true) return false;
                }
            }
            bool any = false;
            if (includeTags != null)
            {
                foreach (var includeTag in includeTags)
                {
                    if (string.IsNullOrWhiteSpace(includeTag)) continue;
                    any = true;
                    if (strValue!.Contains(includeTag)) return true;
                }
            }
            return !any;
        }

        public string[]? Tags(int tagsColumnIndex)
        {
            if (!Utils.IsValidIndex(tagsColumnIndex, Count)) return null;
            string? value = ValueAt(tagsColumnIndex)?.ToString();
            if (string.IsNullOrWhiteSpace(value)) return null;
#if NETSTANDARD2_0
            var split = value.Split([","], StringSplitOptions.RemoveEmptyEntries);
#else
            var split = value.Split(',', StringSplitOptions.RemoveEmptyEntries);
#endif
            return split;
        }

        public string[]? Tags()
        {
            int tagsColumnIndex = Keys.IndexOf(InternalColumnNames.Tags);
            return Tags(tagsColumnIndex);
        }

        public (int CsvIndex, int RowIndex)[]? References(int referencesColumnIndex)
        {
            if (!Utils.IsValidIndex(referencesColumnIndex, Count)) return null;
            string? value = ValueAt(referencesColumnIndex)?.ToString();
            if (string.IsNullOrWhiteSpace(value)) return null;
            var split = value.Split([","], StringSplitOptions.RemoveEmptyEntries);
            return split.Select(ParseIntegers).ToArray();
        }
        public (int CsvIndex, int RowIndex)[]? References()
        {
            int referenceColumnIndex = Keys.IndexOf(InternalColumnNames.References);
            return References(referenceColumnIndex);
        }

        private static (int Left, int Right) ParseIntegers(string input)
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
                return (left, right);
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
            var existingTags = Tags()?.ToHashSet() ?? new HashSet<string>();
            bool added = existingTags.Add(tag);
            if (added)
            {
                this[InternalColumnNames.Tags] = string.Join(",", existingTags);
            }
        }

        public void AddProcessingReference(int referenceCsvId, int referenceRowId)
        {
            var existingReferences = References()?.ToHashSet() ?? new HashSet<(int CsvIndex, int RowIndex)>();
            if (existingReferences.Add((referenceCsvId, referenceRowId)))
            {
                this[InternalColumnNames.References] = string.Join(",", existingReferences);
            }
        }

        public void AddProcessingTags(IEnumerable<string> tags)
        {
            var existingTags = Extensions.Extensions.ToHashSet(this[InternalColumnNames.Tags]?.ToString()?.Split([","], StringSplitOptions.RemoveEmptyEntries));
            foreach (var tag in tags)
            {
                existingTags.Add(tag);
            }
            this[InternalColumnNames.Tags] = string.Join(",", existingTags);
        }

        public void AddProcessingReferences(int referenceCsvId, IEnumerable<int> referenceRowIds)
        {
            var newReferences = referenceRowIds.Select(y => $"{referenceCsvId}-{y}");
            var existingReferences = this[InternalColumnNames.References]?.ToString()?.Split([","], StringSplitOptions.RemoveEmptyEntries);
            if (existingReferences != null)
            {
                newReferences = newReferences.Union(existingReferences);
            }
            this[InternalColumnNames.References] = string.Join(",", newReferences);
        }
    }
}