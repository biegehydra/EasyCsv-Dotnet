﻿using System.Collections.Generic;
using System.IO;
using System.Text;
using EasyCsv.Core.Configuration;

namespace EasyCsv.Core
{
    internal partial class EasyCsv
    {
        internal EasyCsv(EasyCsvConfiguration config)
        {
            Config = config;
        }

        internal EasyCsv(byte[] fileContent, EasyCsvConfiguration config)
        {
            ContentBytes = fileContent;
            ContentStr = Encoding.UTF8.GetString(ContentBytes);
            Config = config;
            CreateCsvContent();
        }

        internal EasyCsv(string fileContent, EasyCsvConfiguration config)
            : this(Encoding.UTF8.GetBytes(fileContent), config)
        {

        }

        internal EasyCsv(Stream fileContentStream, int maxFileSize, EasyCsvConfiguration config)
            : this(ReadStreamToEnd(fileContentStream, maxFileSize), config)
        {
        }

        private static byte[] ReadStreamToEnd(Stream stream, int maxFileSize)
        {
            using var memoryStream = new MemoryStream(maxFileSize);
            stream.CopyTo(memoryStream);
            return memoryStream.ToArray();
        }

        internal EasyCsv(TextReader fileContentReader, EasyCsvConfiguration config)
            : this(fileContentReader.ReadToEnd(), config)
        {

        }

        internal EasyCsv(List<CsvRow> csvContent, EasyCsvConfiguration config)
        {
            CsvContent = CloneContent(csvContent);
            Config = config;
            CalculateContentBytesAndStr();
        }
    }
}