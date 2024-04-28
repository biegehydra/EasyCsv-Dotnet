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
            var parsingStrategy = new SplitColumnStrategy("header2", ["header2", "header3"], "-", false);

            await parsingStrategy.ProcessCsv(easyCsv);

            // Assert
            var headers = easyCsv.GetColumns();
            Assert.AreEqual(3, headers!.Count);
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
            var headers = easyCsv.GetColumns();
            Assert.AreEqual(1, headers!.Count);
            Assert.AreEqual("value1-value2", easyCsv.CsvContent!.First()["header1"]);
        }
    }
}