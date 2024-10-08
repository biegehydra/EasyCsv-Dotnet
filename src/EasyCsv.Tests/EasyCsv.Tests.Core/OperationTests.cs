using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text;
using EasyCsv.Core;
using EasyCsv.Core.Configuration;

namespace EasyCsv.Tests.Core
{
    public class OperationTests
    {
        private class CsvData
        {
         
            public string Header1 { get; set; }
            public string Header2 { get; set; }
        }
        private const string SingleRecordCsv = "header1,header2\nvalue1,value2";
        private static EasyCsvConfiguration DefaultConfig => EasyCsvConfiguration.Instance;

        [Fact]
        public void GetHeaders_ReturnsCorrectHeaders()
        {
            // Arrange
            IEasyCsv easyCsv = new EasyCsv.Core.EasyCsvInternal(SingleRecordCsv, DefaultConfig);

            // Act
            var headers = easyCsv.ColumnNames();

            // Assert
            Assert.NotNull(headers);
            Assert.Equal(2, headers.Length);
            Assert.Contains("header1", headers);
            Assert.Contains("header2", headers);
        }

        [Fact]
        public void ReplaceHeaderRow_ReplacesHeaderSuccessfully()
        {
            // Arrange
            IEasyCsv easyCsv = new EasyCsv.Core.EasyCsvInternal(SingleRecordCsv, DefaultConfig);
            var newHeaders = new List<string> {"newHeader1", "newHeader2"};

            // Act
            easyCsv.ReplaceHeaderRow(newHeaders);
            var updatedHeaders = easyCsv.ColumnNames();

            // Assert
            Assert.NotNull(updatedHeaders);
            Assert.Equal(2, updatedHeaders.Length);
            Assert.Equal(newHeaders, updatedHeaders);
        }

        [Fact]
        public void ReplaceColumn_ReplacesColumnHeaderSuccessfully()
        {
            // Arrange
            var fileContent = "header1,header2\nvalue1,value2";
            IEasyCsv easyCsv = new EasyCsv.Core.EasyCsvInternal(fileContent, DefaultConfig);

            // Act
            easyCsv.ReplaceColumn("header1", "newHeader1");

            // Assert
            var updatedHeaders = easyCsv.ColumnNames();
            Assert.Equal(2, updatedHeaders!.Length);
            Assert.Contains("newHeader1", updatedHeaders);
            Assert.Contains("header2", updatedHeaders);
            Assert.Equal("value1", easyCsv.CsvContent!.First()["newHeader1"]);
            Assert.Equal("value2", easyCsv.CsvContent.First()["header2"]);
        }

        [Fact]
        public void ReplaceColumn_ReplacesColumnHeaderSuccessfullyStrAndBytes()
        {
            // Arrange
            var fileContent = "header1,header2\nvalue1,value2";
            var firstEasyCsv = new EasyCsv.Core.EasyCsvInternal(fileContent, DefaultConfig);

            // Act
            firstEasyCsv.Mutate(x => x.ReplaceColumn("header1", "newHeader1"));

            var newCsv = new EasyCsv.Core.EasyCsvInternal(firstEasyCsv.ContentBytes!, DefaultConfig);

            // Assert
            var updatedHeaders = newCsv.ColumnNames();
            Assert.Equal(2, updatedHeaders!.Length);
            Assert.Contains("newHeader1", updatedHeaders);
            Assert.Contains("header2", updatedHeaders);
            Assert.Equal("value1", newCsv.CsvContent!.First()["newHeader1"]);
            Assert.Equal("value2", newCsv.CsvContent.First()["header2"]);
        }

        [Fact]
        public void GiveColumnsDefaultValues_InsertsDefaultValuesSuccessfully()
        {
            // Arrange
            var fileContent = "header1,header2\nvalue1,value2";
            IEasyCsv easyCsv = new EasyCsv.Core.EasyCsvInternal(fileContent, DefaultConfig);
            var defaultValues = new Dictionary<string, object> {{"header3", "defaultValue"}};

            // Act
            easyCsv.AddColumns(defaultValues);

            // Assert
            var headers = easyCsv.ColumnNames();
            Assert.Equal(3, headers!.Length);
            Assert.Equal("defaultValue", easyCsv.CsvContent!.First()["header3"]);
        }

        [Fact]
        public void AddColumn_InsertsDefaultValueSuccessfully()
        {
            // Arrange
            var fileContent = "header1,header2\nvalue1,value2";
            IEasyCsv easyCsv = new EasyCsv.Core.EasyCsvInternal(fileContent, DefaultConfig);

            // Act
            easyCsv.AddColumn("header3", "defaultValue");

            // Assert
            var headers = easyCsv.ColumnNames();
            Assert.Equal(3, headers!.Length);
            Assert.Equal("defaultValue", easyCsv.CsvContent!.First()["header3"]);

        }

        [Fact]
        public void RemoveColumn_RemovesColumnSuccessfully()
        {
            // Arrange
            var fileContent = "header1,header2,header3\nvalue1,value2,value3";
            IEasyCsv easyCsv = new EasyCsv.Core.EasyCsvInternal(fileContent, DefaultConfig);

            // Act
            easyCsv.RemoveColumn("header2");

            // Assert
            var headers = easyCsv.ColumnNames();
            Assert.Equal(2, headers!.Length);
            Assert.DoesNotContain("header2", headers);
            Assert.False(easyCsv.CsvContent!.First().ContainsKey("header2"));
        }

        [Fact]
        public void RemoveColumns_RemovesColumnsSuccessfully()
        {
            // Arrange
            var fileContent = "header1,header2,header3\nvalue1,value2,value3";
            IEasyCsv easyCsv = new EasyCsv.Core.EasyCsvInternal(fileContent, DefaultConfig);

            // Act
            easyCsv.RemoveColumns(new List<string> { "header2", "header3" });

            // Assert
            var headers = easyCsv.ColumnNames();
            Assert.Single(headers!);
            Assert.DoesNotContain("header2", headers);
            Assert.DoesNotContain("header3", headers);
            Assert.False(easyCsv.CsvContent!.First().ContainsKey("header2"));
            Assert.False(easyCsv.CsvContent.First().ContainsKey("header3"));
        }

        [Fact]
        public void RemoveUnusedHeaders()
        {
            // Arrange
            var fileContent = "header1,header2,header3\nvalue1,value2,";
            IEasyCsv easyCsv = new EasyCsv.Core.EasyCsvInternal(fileContent, DefaultConfig);

            // Act
            easyCsv.RemoveUnusedHeaders();

            // Assert
            var headers = easyCsv.ColumnNames();
            Assert.Equal(2, headers!.Length);
            Assert.DoesNotContain("header3", headers);
        }

        [Fact]
        public void SwapColumnsThrowsBoundsException()
        {
            var fileContent = "header1,header2,header3\nvalue1,value2,value3";
            IEasyCsv easyCsv = new EasyCsv.Core.EasyCsvInternal(fileContent, DefaultConfig);

            Assert.Throws<ArgumentException>(() => easyCsv.Mutate(x => x.SwapColumns(1, 3)));
        }

        [Fact]
        public void SwapColumnsIndex()
        {
            var fileContent = "header1,header2,header3\nvalue1,value2,value3";
            IEasyCsv easyCsv = new EasyCsv.Core.EasyCsvInternal(fileContent, DefaultConfig);

            easyCsv.Mutate(x => x.SwapColumns(0, 2));
            var after = $"header3,header2,header1{Environment.NewLine}value3,value2,value1{Environment.NewLine}";
            Assert.Equal(after, easyCsv.ContentStr);
        }

        [Fact]
        public void SwapColumnsName()
        {
            var fileContent = "header1,header2,header3\nvalue1,value2,value3";
            IEasyCsv easyCsv = new EasyCsv.Core.EasyCsvInternal(fileContent, DefaultConfig);

            easyCsv.Mutate(x => x.SwapColumns("header1", "header3"));
            var after = $"header3,header2,header1{Environment.NewLine}value3,value2,value1{Environment.NewLine}";
            Assert.Equal(after, easyCsv.ContentStr);
        }

        [Fact]
        public void SwapColumnsNameComparer()
        {
            var fileContent = "header1,header2,header3\nvalue1,value2,value3";
            IEasyCsv easyCsv = new EasyCsv.Core.EasyCsvInternal(fileContent, DefaultConfig);

            easyCsv.Mutate(x => x.SwapColumns("HEADER1", "HEADER3", StringComparer.CurrentCultureIgnoreCase));
            var after = $"header3,header2,header1{Environment.NewLine}value3,value2,value1{Environment.NewLine}";
            Assert.Equal(after, easyCsv.ContentStr);
        }

        [Fact]
        public void SwapColumnsNameNoComparer()
        {
            var fileContent = "header1,header2,header3\nvalue1,value2,value3";
            IEasyCsv easyCsv = new EasyCsv.Core.EasyCsvInternal(fileContent, DefaultConfig);

            Assert.Throws<ArgumentException>(() => easyCsv.Mutate(x => x.SwapColumns("HEADER1", "HEADER3")));
        }


        [Fact]
        public void InsertColumn()
        {
            var fileContent = "header1,header2,header3\nvalue1,value2,value3";
            IEasyCsv easyCsv = new EasyCsv.Core.EasyCsvInternal(fileContent, DefaultConfig);

            easyCsv.Mutate(x => x.InsertColumn(1, "header4", "value4"));

            var after = $"header1,header4,header2,header3{Environment.NewLine}value1,value4,value2,value3{Environment.NewLine}";
            Assert.Equal(after, easyCsv.ContentStr);
        }

        [Fact]
        public void MoveColumnIndex()
        {
            var fileContent = "header1,header2,header3\nvalue1,value2,value3";
            IEasyCsv easyCsv = new EasyCsv.Core.EasyCsvInternal(fileContent, DefaultConfig);

            easyCsv.Mutate(x => x.MoveColumn(0, 2));

            var after = $"header2,header3,header1{Environment.NewLine}value2,value3,value1{Environment.NewLine}";
            Assert.Equal(after, easyCsv.ContentStr);

            fileContent = "header1,header2,header3\nvalue1,value2,value3";
            easyCsv = new EasyCsv.Core.EasyCsvInternal(fileContent, DefaultConfig);

            easyCsv.Mutate(x => x.MoveColumn(2, 0));

            after = $"header3,header1,header2{Environment.NewLine}value3,value1,value2{Environment.NewLine}";
            Assert.Equal(after, easyCsv.ContentStr);
        }

        [Fact]
        public void MoveColumnName()
        {
            var fileContent = "header1,header2,header3\nvalue1,value2,value3";
            IEasyCsv easyCsv = new EasyCsv.Core.EasyCsvInternal(fileContent, DefaultConfig);

            easyCsv.Mutate(x => x.MoveColumn("header1", 2));

            var after = $"header2,header3,header1{Environment.NewLine}value2,value3,value1{Environment.NewLine}";
            Assert.Equal(after, easyCsv.ContentStr);

            fileContent = "header1,header2,header3\nvalue1,value2,value3";
            easyCsv = new EasyCsv.Core.EasyCsvInternal(fileContent, DefaultConfig);

            easyCsv.Mutate(x => x.MoveColumn("header3", 0));

            after = $"header3,header1,header2{Environment.NewLine}value3,value1,value2{Environment.NewLine}";
            Assert.Equal(after, easyCsv.ContentStr);
        }

        [Fact]
        public async Task ToList_GeneratesListSuccessfully()
        {
            var fileContent = "header1,header2,header3\nvalue1,value2,value3";
            IEasyCsv easyCsv = new EasyCsv.Core.EasyCsvInternal(fileContent, DefaultConfig);

            var list = await easyCsv.GetRecordsAsync<CsvData>();

            foreach (var csvData in list)
            {
                Assert.False(HasNullOrEmptyStrings(csvData, out _));
            }
        }

        private static bool HasNullOrEmptyStrings(object obj, out IEnumerable<string> emptyProperties)
        {
            var properties = obj.GetType().GetProperties();
            List<string> emptyProps = new ();
            foreach (PropertyInfo property in properties.Where(x => x.PropertyType == typeof(string)))
            {
                string value = (string)property.GetValue(obj);
                if (string.IsNullOrWhiteSpace(value))
                {
                    emptyProps.Add(property.Name);
                }
            }
            emptyProperties = emptyProps;
            return emptyProps.Count > 0;
        }

        private static IEasyCsv CreateCsvWithSampleData()
        {
            var data = "Header1,Header2\nValue1,Value2\nValue3,Value4";

            return new EasyCsv.Core.EasyCsvInternal(data, DefaultConfig);
        }

        [Fact]
        public void Combine_TwoEasyCsvInstances_CombinesData()
        {
            // Arrange
            var csv1 = CreateCsvWithSampleData();
            var csv2 = CreateCsvWithSampleData();

            // Act
            var result = csv1.Combine(csv2);

            // Assert
            Assert.Equal(4, result.CsvContent!.Count);
            Assert.Equal("Value1", result.CsvContent[0]["Header1"]);
            Assert.Equal("Value3", result.CsvContent[1]["Header1"]);
            Assert.Equal("Value1", result.CsvContent[2]["Header1"]);
            Assert.Equal("Value3", result.CsvContent[3]["Header1"]);
        }

        [Fact]
        public void Combine_TwoEasyCsvInstancesWithMismatchedHeaders_DoesNotCombineData()
        {
            // Arrange
            var csv1 = CreateCsvWithSampleData();
            var data = "Header3,Header4\nValue5,Value6";
            var csv2 = new EasyCsv.Core.EasyCsvInternal(data, DefaultConfig);

            // Act
            var result = csv1.Combine(csv2);

            // Assert
            Assert.Equal(2, result.CsvContent!.Count);
            Assert.Equal("Value1", result.CsvContent[0]["Header1"]);
            Assert.Equal("Value3", result.CsvContent[1]["Header1"]);
        }

        [Fact]
        public void Combine_ListOfEasyCsvInstances_CombinesData()
        {
            // Arrange
            var csv1 = CreateCsvWithSampleData();
            var csv2 = CreateCsvWithSampleData();
            var csv3 = CreateCsvWithSampleData();
            var csvList = new List<IEasyCsv?> { csv1, csv2, csv3 };

            // Act
            var result = csv1.Combine(csvList);

            // Assert
            Assert.Equal(8, result.CsvContent.Count);
            Assert.Equal("Value1", result.CsvContent[0]["Header1"]);
            Assert.Equal("Value3", result.CsvContent[1]["Header1"]);
            Assert.Equal("Value1", result.CsvContent[2]["Header1"]);
            Assert.Equal("Value3", result.CsvContent[3]["Header1"]);
            Assert.Equal("Value1", result.CsvContent[4]["Header1"]);
            Assert.Equal("Value3", result.CsvContent[5]["Header1"]);
            Assert.Equal("Value1", result.CsvContent[6]["Header1"]);
            Assert.Equal("Value3", result.CsvContent[7]["Header1"]);
        }
    }
}