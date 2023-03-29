using System.Reflection;
using System.Text;
using EasyCsv.Core.Configuration;

namespace EasyCsv.Tests.Core
{
    public class EasyCsvConstructorTests
    {
        private const string SingleRecordCsv = "header1,header2\nvalue1,value2";
        private static EasyCsvConfiguration DefaultConfig => EasyCsvConfiguration.Instance;
        [Fact]
        public void StringConstructor_CreatesValidInstance()
        {
            // Act
            var easyCsv = new EasyCsv.Core.EasyCsv(SingleRecordCsv, DefaultConfig);

            // Assert
            Assert.NotNull(easyCsv);
            Assert.Single(easyCsv.Content!);
        }

        [Fact]
        public void BytesConstructor_CreatesValidInstance()
        {
            // Arrange
            var fileContentBytes = Encoding.UTF8.GetBytes(SingleRecordCsv);

            // Act
            var easyCsv = new EasyCsv.Core.EasyCsv(fileContentBytes, DefaultConfig);

            // Assert
            Assert.NotNull(easyCsv);
            Assert.Single(easyCsv.Content!);
        }

        [Fact]
        public void TextReaderConstructor_CreatesValidInstance()
        {
            // Arrange
            var textReader = new StringReader(SingleRecordCsv);

            // Act
            var easyCsv = new EasyCsv.Core.EasyCsv(textReader, DefaultConfig);

            // Assert
            Assert.NotNull(easyCsv);
            Assert.Single(easyCsv.Content!);
        }

        [Fact]
        public void StreamConstructor_CreatesValidInstance()
        {
            // Arrange
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(SingleRecordCsv));

            // Act
            var easyCsv = new EasyCsv.Core.EasyCsv(stream, 1024 * 1024 * 15, DefaultConfig);

            // Assert
            Assert.NotNull(easyCsv);
            Assert.Single(easyCsv.Content!);
        }
    }
}