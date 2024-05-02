using System;
using System.Collections.Generic;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;

namespace EasyCsv.Core.Configuration
{
    public class CsvContextProfile
    {
        public IReadOnlyCollection<ClassMap>? ClassMaps { get; set; }
        public IReadOnlyDictionary<Type, ITypeConverter>? TypeConverters { get; set; }
        public IReadOnlyDictionary<Type, TypeConverterOptions>? TypeConvertersOptionsDict { get; set; } 
    }
}
