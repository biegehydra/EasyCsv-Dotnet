using System.Reflection;
using System.Text;
using EasyCsv.Core;
using EasyCsv.Core.Configuration;

namespace EasyCsv.Tests.Core
{
    public class EasyCsvFactoryTests
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
            Assert.Single(easyCsv.Content!);
            Assert.Contains(easyCsv.Content!, x => x.ContainsKey("header1"));
            Assert.Contains(easyCsv.Content!, x => x.ContainsKey("header2"));
            Assert.Contains(easyCsv.Content!, x => x.Values.Contains("value1"));
            Assert.Contains(easyCsv.Content!, x => x.Values.Contains("value2"));
        }
    }
}