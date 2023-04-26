using System.Collections.Generic;
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

        internal EasyCsv(Stream fileContentStream, EasyCsvConfiguration config, bool calculateContentByteAndStr = true)
        {
            Config = config;
            CreateCsvContent(fileContentStream);
            if (calculateContentByteAndStr)
            {
                CalculateContentBytesAndStr();
            }
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