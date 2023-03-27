using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Text;

namespace EasyCsv.Tests.Core
{
    public class EasyCsvCoreTests
    {
        private class CsvData
        {
         
            public string Header1 { get; set; }
            public string Header2 { get; set; }
        }
        private const string SingleRecordCsv = "header1,header2\nvalue1,value2";

        [Fact]
        public void CsvFileContentConstructor_CreatesValidInstance()
        {
            // Act
            var easyCsv = new EasyCsv.Core.EasyCsv(SingleRecordCsv);

            // Assert
            Assert.NotNull(easyCsv);
            Assert.Single(easyCsv.CsvContent);
        }

        [Fact]
        public void CsvBytesConstructor_CreatesValidInstance()
        {
            // Arrange
            var fileContentBytes = Encoding.UTF8.GetBytes(SingleRecordCsv);

            // Act
            var easyCsv = new EasyCsv.Core.EasyCsv(fileContentBytes);

            // Assert
            Assert.NotNull(easyCsv);
            Assert.Single(easyCsv.CsvContent);
        }


        [Fact]
        public void GetHeaders_ReturnsCorrectHeaders()
        {
            // Arrange
            var easyCsv = new EasyCsv.Core.EasyCsv(SingleRecordCsv);

            // Act
            var headers = easyCsv.GetHeaders();

            // Assert
            Assert.NotNull(headers);
            Assert.Equal(2, headers.Count());
            Assert.Contains("header1", headers);
            Assert.Contains("header2", headers);
        }

        [Fact]
        public void ReplaceHeaderRow_ReplacesHeaderSuccessfully()
        {
            // Arrange
            var easyCsv = new EasyCsv.Core.EasyCsv(SingleRecordCsv);
            var newHeaders = new List<string> {"newHeader1", "newHeader2"};

            // Act
            easyCsv.ReplaceHeaderRow(newHeaders);
            var updatedHeaders = easyCsv.GetHeaders();

            // Assert
            Assert.NotNull(updatedHeaders);
            Assert.Equal(2, updatedHeaders.Count());
            Assert.Equal(newHeaders, updatedHeaders);
        }

        [Fact]
        public void ReplaceColumn_ReplacesColumnHeaderSuccessfully()
        {
            // Arrange
            var fileContent = "header1,header2\nvalue1,value2";
            var easyCsv = new EasyCsv.Core.EasyCsv(fileContent);

            // Act
            easyCsv.ReplaceColumn("header1", "newHeader1");

            // Assert
            var updatedHeaders = easyCsv.GetHeaders();
            Assert.Equal(2, updatedHeaders.Count());
            Assert.Contains("newHeader1", updatedHeaders);
            Assert.Contains("header2", updatedHeaders);
            Assert.Equal("value1", easyCsv.CsvContent.First()["newHeader1"]);
            Assert.Equal("value2", easyCsv.CsvContent.First()["header2"]);
        }

        [Fact]
        public void GiveColumnsDefaultValues_InsertsDefaultValuesSuccessfully()
        {
            // Arrange
            var fileContent = "header1,header2\nvalue1,value2";
            var easyCsv = new EasyCsv.Core.EasyCsv(fileContent);
            var defaultValues = new Dictionary<string, string> {{"header3", "defaultValue"}};

            // Act
            easyCsv.AddColumns(defaultValues);

            // Assert
            var headers = easyCsv.GetHeaders();
            Assert.Equal(3, headers!.Count());
            Assert.Equal("defaultValue", easyCsv.CsvContent.First()["header3"]);
        }

        [Fact]
        public void GiveColumnDefaultValue_InsertsDefaultValueSuccessfully()
        {
            // Arrange
            var fileContent = "header1,header2\nvalue1,value2";
            var easyCsv = new EasyCsv.Core.EasyCsv(fileContent);

            // Act
            easyCsv.AddColumn("header3", "defaultValue");

            // Assert
            var headers = easyCsv.GetHeaders();
            Assert.Equal(3, headers.Count());
            Assert.Equal("defaultValue", easyCsv.CsvContent.First()["header3"]);

        }

        [Fact]
        public void RemoveColumn_RemovesColumnSuccessfully()
        {
            // Arrange
            var fileContent = "header1,header2,header3\nvalue1,value2,value3";
            var easyCsv = new EasyCsv.Core.EasyCsv(fileContent);

            // Act
            easyCsv.RemoveColumn("header2");

            // Assert
            var headers = easyCsv.GetHeaders();
            Assert.Equal(2, headers.Count());
            Assert.DoesNotContain("header2", headers);
            Assert.False(easyCsv.CsvContent.First().ContainsKey("header2"));
        }

        [Fact]
        public void RemoveColumns_RemovesColumnsSuccessfully()
        {
            // Arrange
            var fileContent = "header1,header2,header3\nvalue1,value2,value3";
            var easyCsv = new EasyCsv.Core.EasyCsv(fileContent);

            // Act
            easyCsv.RemoveColumns(new List<string> { "header2", "header3" });

            // Assert
            var headers = easyCsv.GetHeaders();
            Assert.Single(headers);
            Assert.DoesNotContain("header2", headers);
            Assert.DoesNotContain("header3", headers);
            Assert.False(easyCsv.CsvContent.First().ContainsKey("header2"));
            Assert.False(easyCsv.CsvContent.First().ContainsKey("header3"));
        }

        [Fact]
        public async Task ToList_GeneratesListSuccessfully()
        {
            var fileContent = "header1,header2,header3\nvalue1,value2,value3";
            var easyCsv = new EasyCsv.Core.EasyCsv(fileContent);

            var list = await easyCsv.GetRecords<CsvData>();

            foreach (var csvData in list)
            {
                Assert.False(HasNullOrEmptyStrings(csvData, out var emptyProperties));
            }
        }
        private static bool HasNullOrEmptyStrings(object obj, out IEnumerable<string> emptyProperties)
        {
            PropertyInfo[] properties = obj.GetType().GetProperties();
            List<string> emptyProps = new List<string>();
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
    }
}