﻿using System;
using CsvHelper.Configuration;
using CsvHelper;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CsvHelper.Delegates;
using EasyCsv.Core.Configuration;

namespace EasyCsv.Core
{
    internal record CsvConfigurationWrapper : CsvConfiguration, IReaderConfiguration
    {
        internal readonly Dictionary<(string, int), string> AssignedHeaders = new();
        private readonly CsvConfiguration _config;
        private int _i;
        private readonly bool _giveEmptyHeadersNames;

        public CsvConfigurationWrapper(CsvConfiguration config, bool giveEmptyHeadersNames) : base(config.CultureInfo)
        {
            _config = config;
            _giveEmptyHeadersNames = giveEmptyHeadersNames;
        }

        public void Validate()
        {
            _config.Validate();
        }
        public CultureInfo CultureInfo => _config.CultureInfo;
        public bool CacheFields => _config.CacheFields;
        public string NewLine => _config.NewLine;
        public bool IsNewLineSet => _config.IsNewLineSet;
        public CsvMode Mode => _config.Mode;
        public int BufferSize => _config.BufferSize;
        public int ProcessFieldBufferSize => _config.ProcessFieldBufferSize;
        public bool CountBytes => _config.CountBytes;
        public Encoding Encoding => _config.Encoding;
        public BadDataFound BadDataFound => _config.BadDataFound;
        public double MaxFieldSize => _config.MaxFieldSize;
        public bool LineBreakInQuotedFieldIsBadData => _config.LineBreakInQuotedFieldIsBadData;
        public char Comment => _config.Comment;
        public bool AllowComments => _config.AllowComments;
        public bool IgnoreBlankLines => _config.IgnoreBlankLines;
        public char Quote => _config.Quote;
        public string Delimiter => _config.Delimiter;
        public bool DetectDelimiter => _config.DetectDelimiter;
        public GetDelimiter GetDelimiter => _config.GetDelimiter;
        public string[] DetectDelimiterValues => _config.DetectDelimiterValues;
        public char Escape => _config.Escape;
        public TrimOptions TrimOptions => _config.TrimOptions;
        public char[] WhiteSpaceChars => _config.WhiteSpaceChars;
        public bool ExceptionMessagesContainRawData => _config.ExceptionMessagesContainRawData;
        public bool HasHeaderRecord => _config.HasHeaderRecord;
        public HeaderValidated HeaderValidated => _config.HeaderValidated;
        public MissingFieldFound MissingFieldFound => _config.MissingFieldFound;
        public ReadingExceptionOccurred ReadingExceptionOccurred => _config.ReadingExceptionOccurred;
        public PrepareHeaderForMatch PrepareHeaderForMatch
        {
            get
            {
                if (_giveEmptyHeadersNames)
                {
                    return x => string.IsNullOrWhiteSpace(x.Header) ? $"EmptyHeader{x.FieldIndex}" : _config.PrepareHeaderForMatch(x);
                }
                return _config.PrepareHeaderForMatch;
            }
        }

        public ShouldUseConstructorParameters ShouldUseConstructorParameters => _config.ShouldUseConstructorParameters;
        public GetConstructor GetConstructor => _config.GetConstructor;

        public GetDynamicPropertyName GetDynamicPropertyName => args =>
        {
            if (args.Context.Reader.HeaderRecord != null)
            {
                var origHeader = args.Context.Reader.HeaderRecord[args.FieldIndex];
                var prepareHeaderForMatchArgs = new PrepareHeaderForMatchArgs(origHeader, args.FieldIndex);
                origHeader = args.Context.Reader.Configuration.PrepareHeaderForMatch(prepareHeaderForMatchArgs);

                // All rows after first
                // O(1)
                if (AssignedHeaders.TryGetValue((origHeader, args.FieldIndex), out var assignedHeader))
                {
                    // all headers should be assigned after first row
                    return assignedHeader;
                }

                assignedHeader = origHeader;
                // First row only
                // O(N)
                for (int i = 2; AssignedHeaders.Values.Contains(assignedHeader); i++)
                {
                    assignedHeader = $"{origHeader}{i}";
                }
                AssignedHeaders.Add((origHeader, args.FieldIndex), assignedHeader);

                // In  "header,header,header,header"
                // Out "header,header2,header3,header4"
                // Why? Because it feels right
                return assignedHeader;
            }

            return $"Empty_Header{++_i}";
        };

        public bool IgnoreReferences => _config.IgnoreReferences;
        public ShouldSkipRecord ShouldSkipRecord => _config.ShouldSkipRecord ?? new (x => false);
        public bool IncludePrivateMembers => _config.IncludePrivateMembers;
        public ReferenceHeaderPrefix ReferenceHeaderPrefix => _config.ReferenceHeaderPrefix ?? new (x => x.MemberName);
        public bool DetectColumnCountChanges => _config.DetectColumnCountChanges;
        public MemberTypes MemberTypes => _config.MemberTypes;
    }

    internal partial class EasyCsvInternal
    {
        private void CreateCsvContent(Stream stream)
        {
            var wrapper = new CsvConfigurationWrapper(Config.CsvHelperConfig, Config.GiveEmptyHeadersNames);
            using var reader = new StreamReader(stream, Encoding.Default);
            using var csv = new CsvReader(reader, wrapper);
            csv.Read();
            csv.ReadHeader();
            CsvContent = csv.GetRecords<dynamic>().Select(x => new CsvRow((IDictionary<string, object?>)x)).ToList();
        }

        private void CreateCsvContent()
        {
            if (ContentBytes == null) return;
            var memoryStream = new MemoryStream(ContentBytes);
            CreateCsvContent(memoryStream);
        }

        internal async Task CreateCsvContentInBackGround()
        {
            if (ContentBytes == null) return;
            await Task.Run(CreateCsvContent);
        }

        internal static IEasyCsv FromObjects<T>(IEnumerable<T> objects, EasyCsvConfiguration config, CsvContextProfile? csvContextProfile = null)
        {
            using var memoryStream = new MemoryStream();
            using (var streamWriter = new StreamWriter(memoryStream, Encoding.UTF8, 1024, leaveOpen: true))
            using (var csvWriter = new CsvWriter(streamWriter, config.CsvHelperConfig))
            {
                AddSettingsToCsvContext<T>(csvWriter.Context, csvContextProfile);

                csvWriter.WriteRecords(objects);
                streamWriter.Flush(); 

                memoryStream.Position = 0; 
            }

            using var streamReader = new StreamReader(memoryStream);
            var csvContent = streamReader.ReadToEnd();

            return new EasyCsvInternal(csvContent, config);
        }
        internal static async Task<IEasyCsv> FromObjectsAsync<T>(IEnumerable<T> objects, EasyCsvConfiguration config, CsvContextProfile? csvContextProfile = null)
        {
            using var memoryStream = new MemoryStream();
#if NETSTANDARD2_1_OR_GREATER
            await using (var streamWriter = new StreamWriter(memoryStream, Encoding.UTF8, 1024, leaveOpen: true))
            await using (var csvWriter = new CsvWriter(streamWriter, config.CsvHelperConfig))
#else
            using var streamWriter = new StreamWriter(memoryStream, Encoding.UTF8, 1024, leaveOpen: true);
            using var csvWriter = new CsvWriter(streamWriter, config.CsvHelperConfig);
#endif
            {
                AddSettingsToCsvContext<T>(csvWriter.Context, csvContextProfile);

                await csvWriter.WriteRecordsAsync(objects);
                await streamWriter.FlushAsync(); // Ensure all data is written to the underlying MemoryStream

                memoryStream.Position = 0; // Reset the position of the MemoryStream to the beginning for reading
            }

            // Read directly from the original MemoryStream
            using var streamReader = new StreamReader(memoryStream);
            var csvContent = await streamReader.ReadToEndAsync();

            return new EasyCsvInternal(csvContent, config);
        }

        public async Task CalculateContentBytesAndStrAsync()
        {
#if NETSTANDARD2_1_OR_GREATER
            await using var writer = new StringWriter();
            await using var csvWriter = new CsvWriter(writer, Config.CsvHelperConfig);
#else
            using var writer = new StringWriter();
            using var csvWriter = new CsvWriter(writer, Config.CsvHelperConfig);
#endif
            var dynamicContent = CsvContent?.Cast<dynamic>();
            await csvWriter.WriteRecordsAsync(dynamicContent);
            ContentStr = writer.ToString();
            ContentBytes = Encoding.UTF8.GetBytes(ContentStr);
        }

        public void CalculateContentBytesAndStr()
        {
            using var writer = new StringWriter();
            using var csv = new CsvWriter(writer, Config.CsvHelperConfig);
            var dynamicContent = CsvContent?.Cast<dynamic>();
            csv.WriteRecords(dynamicContent);
            ContentStr = writer.ToString();
            ContentBytes = Encoding.UTF8.GetBytes(ContentStr);
        }

        public async Task<List<T>> GetRecordsAsync<T>(bool strict = false, CsvContextProfile? csvContextProfile = null)
        {
            var records = new List<T>();
            if (CsvContent == null!) return records;

            await CalculateContentBytesAndStrAsync();
            if (string.IsNullOrEmpty(ContentStr)) return records;

            if (strict)
            {
                await ReadRecordsStrict();
            }
            else
            {
                await ReadRecordsNotStrict();
            }
            return records;

            async Task ReadRecordsStrict()
            {
            using var reader = new StreamReader(new MemoryStream(ContentBytes!), Encoding.UTF8);
            using var csvReader = new CsvReader(reader, Config.CsvHelperConfig);

            AddSettingsToCsvContext<T>(csvReader.Context, csvContextProfile);

            await foreach (var record in csvReader.GetRecordsAsync<T>())
            {
                records.Add(record);
            }
            }
            async Task ReadRecordsNotStrict()
            {
                var config = new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    HeaderValidated = null,
                    MissingFieldFound = null,
                    PrepareHeaderForMatch = args => Regex.Replace(args.Header, @"\W", "").ToLower(CultureInfo.InvariantCulture)
                };
                using var reader = new StreamReader(new MemoryStream(ContentBytes!), Encoding.UTF8);
                using var csvReader = new CsvReader(reader, config);

                AddSettingsToCsvContext<T>(csvReader.Context, csvContextProfile);

                try
                {
                    await foreach (var record in csvReader.GetRecordsAsync<T>())
                    {
                        records.Add(record);
                    }
                }
                catch (Exception ex)
                {
                    Console.Write(ex);
                }
            }
        }

        private static void AddSettingsToCsvContext<T>(CsvContext context, CsvContextProfile? csvContextProfile = null)
        {
            if (csvContextProfile?.TypeConverters != null)
            {
                foreach (var typeConverter in csvContextProfile.TypeConverters)
                {
                    context.TypeConverterCache.AddConverter(typeConverter.Key, typeConverter.Value);
                }
            }

            if (csvContextProfile?.ClassMaps != null)
            {
                foreach (var classMap in csvContextProfile.ClassMaps)
                {
                    context.RegisterClassMap(classMap);
                }
            }

            if (csvContextProfile?.TypeConvertersOptionsDict != null)
            {
                foreach (var typeConverterOption in csvContextProfile.TypeConvertersOptionsDict)
                {
                    context.TypeConverterOptionsCache.AddOptions(typeConverterOption.Key, typeConverterOption.Value);
                }
            }
        }

        public async Task<List<T>> GetRecordsAsync<T>(PrepareHeaderForMatch prepareHeaderForMatch, CsvContextProfile? csvContextProfile = null)
        {
            var records = new List<T>();
            if (CsvContent == null) return records;

            await CalculateContentBytesAndStrAsync();
            if (string.IsNullOrEmpty(ContentStr)) return records;
            await ReadRecords();
            return records;

            async Task ReadRecords()
            {
                var config = new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    PrepareHeaderForMatch = prepareHeaderForMatch,
                };
                using var reader = new StreamReader(new MemoryStream(ContentBytes!), Encoding.UTF8);
                using var csvReader = new CsvReader(reader, config);

                AddSettingsToCsvContext<T>(csvReader.Context, csvContextProfile);

                await foreach (var record in csvReader.GetRecordsAsync<T>())
                {
                    records.Add(record);
                }
            }
        }

        public async Task<List<T>> GetRecordsAsync<T>(CsvConfiguration csvConfig, CsvContextProfile? csvContextProfile = null)
        {
            var records = new List<T>();
            if (CsvContent == null) return records;

            await CalculateContentBytesAndStrAsync();
            if (string.IsNullOrEmpty(ContentStr)) return records;
            await ReadRecords();
            return records;

            async Task ReadRecords()
            {
                using var reader = new StreamReader(new MemoryStream(ContentBytes!), Encoding.UTF8);
                using var csvReader = new CsvReader(reader, csvConfig);

                AddSettingsToCsvContext<T>(csvReader.Context, csvContextProfile);

                await foreach (var record in csvReader.GetRecordsAsync<T>())
                {
                    records.Add(record);
                }
            }
        }

        public async IAsyncEnumerable<T> ReadRecordsAsync<T>(CsvConfiguration config, CsvContextProfile? csvContextProfile = null)
        {
            using var reader = new StreamReader(new MemoryStream(ContentBytes!), Encoding.UTF8);
            using var csvReader = new CsvReader(reader, config);

            AddSettingsToCsvContext<T>(csvReader.Context, csvContextProfile);

            await foreach (var record in csvReader.GetRecordsAsync<T>())
            {
                yield return record;
            }
        }
    }
}
