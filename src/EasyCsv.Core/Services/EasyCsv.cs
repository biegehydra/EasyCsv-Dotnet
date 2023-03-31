using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using EasyCsv.Core.Configuration;

[assembly: InternalsVisibleTo("EasyCsv.Files")]
[assembly: InternalsVisibleTo("EasyCsv.Tests.Files")]
[assembly: InternalsVisibleTo("EasyCsv.Tests.Core")]
namespace EasyCsv.Core
{
    internal partial class EasyCsv : IEasyCsv
    {
        private EasyCsvConfiguration Config { get; }
        public byte[]? ContentBytes { get; internal set; }
        public string? ContentStr { get; internal set; }
        public List<IDictionary<string, object>>? Content { get; private set; }

        public int GetRowCount() => Content?.Count ?? 0;

        public bool ContainsHeader(string headerField, bool caseInsensitive = false) => GetHeaders()?.Contains(caseInsensitive ? headerField!.ToLower(CultureInfo.InvariantCulture) : headerField) ?? false;

        public bool ContainsRow(IDictionary<string, object> row)
        {
            return Content?.Any(r => RowsEqual(r, row)) ?? false;
        }

        private static bool RowsEqual(IDictionary<string, object> row1, IDictionary<string, object> row2)
        {
            return row1.Count == row2.Count && !row1.Except(row2).Any();
        }

        public List<string>? GetHeaders()
        {
            return Content?.FirstOrDefault()?.Keys.ToList();
        }

        private List<IDictionary<string, object>> CloneContent(List<IDictionary<string, object>> content)
        {
            return content.Select(row => (dynamic)row).Select(x => (IDictionary<string, object>)x).ToList();
        }

        public IEasyCsv Clone()
        {
            return new EasyCsv(CloneContent(Content), Config);
        }


        private string Normalize(string header)
        {
            return Config.NormalizeFields ? Config.NormalizeFieldsFunc(header) : header;
        }

        object ICloneable.Clone()
        {
            return Clone();
        }
    }
}
