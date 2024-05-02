using System.Text;
using EasyCsv.Core;
using EasyCsv.Core.Configuration;

namespace EasyCsv.Tests.Core
{
    public class ConstructorTests
    {
        private const string SingleRecordCsv = "header1,header2\nvalue1,value2";
        private static EasyCsvConfiguration DefaultConfig => EasyCsvConfiguration.Instance;
        [Fact]
        public void StringConstructor_CreatesValidInstance()
        {
            // Act
            IEasyCsv easyCsv = new EasyCsv.Core.EasyCsvInternal(SingleRecordCsv, DefaultConfig);

            // Assert
            Assert.NotNull(easyCsv);
            Assert.Single(easyCsv.CsvContent!);
        }

        [Fact]
        public void BytesConstructor_CreatesValidInstance()
        {
            // Arrange
            var fileContentBytes = Encoding.UTF8.GetBytes(SingleRecordCsv);

            // Act
            IEasyCsv easyCsv = new EasyCsv.Core.EasyCsvInternal(fileContentBytes, DefaultConfig);

            // Assert
            Assert.NotNull(easyCsv);
            Assert.Single(easyCsv.CsvContent!);
        }

        [Fact]
        public void TextReaderConstructor_CreatesValidInstance()
        {
            // Arrange
            var textReader = new StringReader(SingleRecordCsv);

            // Act
            IEasyCsv easyCsv = new EasyCsv.Core.EasyCsvInternal(textReader, DefaultConfig);

            // Assert
            Assert.NotNull(easyCsv);
            Assert.Single(easyCsv.CsvContent!);
        }

        [Fact]
        public void StreamConstructor_CreatesValidInstance()
        {
            // Arrange
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(SingleRecordCsv));

            // Act
            IEasyCsv easyCsv = new EasyCsv.Core.EasyCsvInternal(stream, DefaultConfig);

            // Assert
            Assert.NotNull(easyCsv);
            Assert.Single(easyCsv.CsvContent!);
        }
    }
}