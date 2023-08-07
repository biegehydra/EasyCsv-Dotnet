using System.Reflection;
using System.Text;
using EasyCsv.Core;
using EasyCsv.Core.Configuration;

namespace EasyCsv.Tests.Core
{
    public class FactoryTests
    {
        private const string SingleRecordCsv = "header1,header2\nvalue1,value2";
        private static EasyCsvConfiguration DefaultConfig => EasyCsvConfiguration.Instance;
        [Fact]
        public void StringConstructor_CreatesValidInstance()
        {
            // Act
            var easyCsv = EasyCsvFactory.FromString(SingleRecordCsv, DefaultConfig);

            // Assert
            AssertValidEasyCsv(easyCsv);
        }

        [Fact]
        public void AsyncStringConstructor_CreatesValidInstance()
        {
            // Act
            var easyCsv = EasyCsvFactory.FromString(SingleRecordCsv, DefaultConfig);

            // Assert
            AssertValidEasyCsv(easyCsv);
        }

        [Fact]
        public void BytesConstructor_CreatesValidInstance()
        {
            // Act
            var easyCsv = EasyCsvFactory.FromString(SingleRecordCsv, DefaultConfig);

            // Assert
            AssertValidEasyCsv(easyCsv);
        }

        [Fact]
        public void AsyncBytesConstructor_CreatesValidInstance()
        {
            // Act
            var easyCsv = EasyCsvFactory.FromString(SingleRecordCsv, DefaultConfig);

            // Assert
            AssertValidEasyCsv(easyCsv);
        }

        [Fact]
        public void StreamConstructor_CreatesValidInstance()
        {
            // Act
            var easyCsv = EasyCsvFactory.FromString(SingleRecordCsv, DefaultConfig);

            // Assert
            AssertValidEasyCsv(easyCsv);
        }

        [Fact]
        public void AsyncStreamConstructor_CreatesValidInstance()
        {
            // Act
            var easyCsv = EasyCsvFactory.FromString(SingleRecordCsv, DefaultConfig);

            // Assert
            AssertValidEasyCsv(easyCsv);
        }

        private static void AssertValidEasyCsv(IEasyCsv easyCsv)
        {
            Assert.NotNull(easyCsv);
            Assert.Single(easyCsv.CsvContent!);
            Assert.Contains(easyCsv.CsvContent!, x => x.ContainsKey("header1"));
            Assert.Contains(easyCsv.CsvContent!, x => x.ContainsKey("header2"));
            Assert.Contains(easyCsv.CsvContent!, x => x.Values.Contains("value1"));
            Assert.Contains(easyCsv.CsvContent!, x => x.Values.Contains("value2"));
        }

        private class TestObject
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        [Fact]
        public void FromObjects_CreatesCsvContentFromObjects()
        {
            // Arrange
            var config = new EasyCsvConfiguration();
            var objects = new List<TestObject>
            {
                new TestObject { Id = 1, Name = "Alice" },
                new TestObject { Id = 2, Name = "Bob" }
            };

            // Act
            var easyCsv = EasyCsvFactory.FromObjects(objects, config);

            // Assert
            Assert.Contains("Id,Name", easyCsv!.ContentStr);
            Assert.Contains("1,Alice", easyCsv.ContentStr);
            Assert.Contains("2,Bob", easyCsv.ContentStr);
        }

        [Fact]
        public async Task FromObjectsAsync_CreatesCsvContentFromObjectsAsync()
        {
            // Arrange
            var config = new EasyCsvConfiguration();
            var objects = new List<TestObject>
            {
                new TestObject { Id = 1, Name = "Alice" },
                new TestObject { Id = 2, Name = "Bob" }
            };

            // Act
            var easyCsv = await EasyCsvFactory.FromObjectsAsync(objects, config);

            // Assert
            Assert.Contains("Id,Name", easyCsv!.ContentStr);
            Assert.Contains("1,Alice", easyCsv.ContentStr);
            Assert.Contains("2,Bob", easyCsv.ContentStr);
        }
    }
}