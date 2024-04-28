using EasyCsv.Core;
using EasyCsv.Core.Configuration;
using EasyCsv.Parsing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EasyCsv.Tests.Parsing
{
    [TestClass]
    public class CsvParsingTests
    {
        [TestMethod]
        public async Task SplitColumnsStrategy()
        {
            // Arrange
            var fileContent = "header1,header2\nvalue1,value2-value3";
            IEasyCsv easyCsv = new EasyCsv.Core.EasyCsv(fileContent, EasyCsvConfiguration.Instance);
            var parsingStrategy = new StringSplitColumnStrategy("header2", ["header2", "header3"], "-", false);

            await parsingStrategy.ProcessCsv(easyCsv);

            // Assert
            var columns = easyCsv.ColumnNames();
            Assert.AreEqual(3, columns!.Length);
            Assert.AreEqual("value2", easyCsv.CsvContent!.First()["header2"]);
            Assert.AreEqual("value3", easyCsv.CsvContent!.First()["header3"]);
        }

        [TestMethod]
        public async Task JoinColumnStrategy()
        {
            // Arrange
            var fileContent = "header1,header2\nvalue1,value2";
            IEasyCsv easyCsv = new Core.EasyCsv(fileContent, EasyCsvConfiguration.Instance);
            var parsingStrategy = new JoinColumnsStrategy(["header1", "header2"], "header1", "-", true);

            await parsingStrategy.ProcessCsv(easyCsv);

            // Assert
            var headers = easyCsv.ColumnNames();
            Assert.AreEqual(1, headers!.Length);
            Assert.AreEqual("value1-value2", easyCsv.CsvContent!.First()["header1"]);
        }

        [TestMethod]
        public async Task DivideAndReplicate()
        {
            // Arrange
            var fileContent = "Name,Email\nJohn,\"jane.smith@gmail.com,joe.smith@gmail.com\"";
            IEasyCsv easyCsv = new Core.EasyCsv(fileContent, EasyCsvConfiguration.Instance);
            var parsingStrategy = new DivideAndReplicate("Email", x => x?.ToString()?.Split(',')?.Cast<object>().ToArray());

            await parsingStrategy.ProcessCsv(easyCsv);

            // Assert
            var rowCount = easyCsv.RowCount();
            Assert.AreEqual(2, rowCount);
            Assert.AreEqual("jane.smith@gmail.com", easyCsv.CsvContent!.First()["Email"]);
            Assert.AreEqual("joe.smith@gmail.com", easyCsv.CsvContent!.Skip(1).First()["Email"]);
        }

        [TestMethod]
        public async Task TestMergeCsvs()
        {
            string[] newColumnNames = ["additionalHeader2"];
            var columnMapping = new ColumnMapping[]
            {
                new ColumnMapping("header1", "additionalHeader3"),
                new ColumnMapping("header2", "additionalHeader1")
            };
            var baseCsvStr = "header1,header2\nvalue1,value2";
            var additionalCsvStr = "additionalHeader1,additionalHeader2,additionalHeader3\n" +
                                "additionalValue1,additionalValue2,additionalValue3\n" +
                                "additionalValue4,additionalValue5,additionalValue6";
            var baseCsv = await EasyCsvFactory.FromStringAsync(baseCsvStr);
            var additionalCsv = await EasyCsvFactory.FromStringAsync(additionalCsvStr);
            Assert.IsNotNull(baseCsv);
            Assert.IsNotNull(additionalCsv);

            var mergeStrategy = new MergeCsvsStrategy(new MergeConfig(newColumnNames, columnMapping));
            var merged = await mergeStrategy.Merge(baseCsv, additionalCsv);

            string mergedExpected = @"header1,header2,additionalHeader2
value1,value2,
additionalValue3,additionalValue1,additionalValue2
additionalValue6,additionalValue4,additionalValue5
";
            Assert.AreEqual(mergedExpected, merged.ContentStr);
        }
    }
}