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
        /// Determines whether to normalize fields when generating <code>ContentStr</code> and <code>ContentBytes</code>
        /// </summary>
        public bool NormalizeFields { get; } = DefaultEasyConfiguration.NormalizeHeaders;

        public Func<string, string> NormalizeFieldsFunc { get; } = DefaultEasyConfiguration.NormalizeHeadersFunc;

        public CsvConfiguration CsvHelperConfig { get; } = DefaultEasyConfiguration.CsvConfiguration;

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

    public static class DefaultEasyConfiguration
    {
        /// <summary>
        /// If true will use the <see cref="NormalizeHeadersFunc"/> to normalize fields when generating <code>ContentStr</code> and <code>ContentBytes</code>
        /// </summary>
        public const bool NormalizeHeaders = false;
        /// <summary>
        /// Function to be used to normalize fields when generating <code>ContentStr</code> and <code>ContentBytes</code>. Only called when <see cref="NormalizeHeaders"/> is true
        /// </summary>
        public static readonly Func<string, string> NormalizeHeadersFunc = header => header.Replace(" ", "").Replace("\"", "").ToLower(CultureInfo.InvariantCulture);
        /// <summary>
        /// CsvHelper configuration that will be used through EasyCsv to read and write csv data when not explicitly given.
        /// </summary>
        public static readonly CsvConfiguration CsvConfiguration = new CsvConfiguration(CultureInfo.InvariantCulture);
    }
}