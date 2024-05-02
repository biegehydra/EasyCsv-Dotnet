using System.Collections.Generic;
using System.IO;
using System.Text;
using EasyCsv.Core.Configuration;

namespace EasyCsv.Core
{
    internal partial class EasyCsvInternal
    {
        internal EasyCsvInternal(EasyCsvConfiguration config)
        {
            Config = config;
        }

        internal EasyCsvInternal(byte[] fileContent, EasyCsvConfiguration config)
        {
            ContentBytes = fileContent;
            ContentStr = Encoding.UTF8.GetString(ContentBytes);
            Config = config;
            CreateCsvContent();
        }

        internal EasyCsvInternal(string fileContent, EasyCsvConfiguration config)
            : this(Encoding.UTF8.GetBytes(fileContent), config)
        {

        }

        internal EasyCsvInternal(Stream fileContentStream, EasyCsvConfiguration config, bool calculateContentByteAndStr = true)
        {
            Config = config;
            CreateCsvContent(fileContentStream);
            if (calculateContentByteAndStr)
            {
                CalculateContentBytesAndStr();
            }
        }

        internal EasyCsvInternal(TextReader fileContentReader, EasyCsvConfiguration config)
            : this(fileContentReader.ReadToEnd(), config)
        {

        }

        internal EasyCsvInternal(List<CsvRow> csvContent, EasyCsvConfiguration config)
        {
            CsvContent = CloneContent(csvContent);
            Config = config;
            CalculateContentBytesAndStr();
        }
    }
}