using System;
using System.Globalization;
using CsvHelper.Configuration;

namespace EasyCsv.Core.Configuration
{
    public class EasyCsvConfiguration
    {
        private static EasyCsvConfiguration? _instance;
        private static readonly object Lock = new object();

        /// <summary>
        /// If true, PrepareHeadersWithMatch will be overriden and
        /// will set empty headers to "EmptyHeaders{FieldIndex}".
        /// Without this setting, multiple empty headers will throw an exception
        /// </summary>
        public bool GiveEmptyHeadersNames { get; set; } = true;

        /// <summary>
        /// CsvHelper configuration that will be used through EasyCsv to read and write csv data when not explicitly given.
        /// </summary>
        public CsvConfiguration CsvHelperConfig { get; set; } = DefaultEasyConfiguration.CsvConfiguration;

        public static EasyCsvConfiguration Instance
        {
            get
            {
                if (_instance != null) return _instance;
                lock (Lock)
                {
                    _instance ??= new EasyCsvConfiguration();
                }
                return _instance;
            }
        }

        public static void SetGlobalConfig(EasyCsvConfiguration configuration)
        {
            _instance = configuration;
        }

    }

    internal static class DefaultEasyConfiguration
    {
        /// <summary>
        /// CsvHelper configuration that will be used through EasyCsv to read and write csv data when not explicitly given.
        /// </summary>
        public static readonly CsvConfiguration CsvConfiguration = new CsvConfiguration(CultureInfo.CurrentCulture, typeof(object));
    }
}