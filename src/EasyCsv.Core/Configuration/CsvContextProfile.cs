using System;
using System.Collections.Generic;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;

namespace EasyCsv.Core.Configuration
{
    public class CsvContextProfile
    {
        public List<ClassMap> ClassMaps { get; set; }
        public Dictionary<Type, ITypeConverter> TypeConverters { get; set; }
        public Dictionary<Type, TypeConverterOptions> TypeConvertersOptionsDict { get; set; } 
    }
}
