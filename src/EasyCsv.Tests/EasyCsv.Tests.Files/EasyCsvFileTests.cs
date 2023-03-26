using Microsoft.AspNetCore.Http;
using Moq;
using System.Text;
using EasyCsv.Files;
using Microsoft.AspNetCore.Components.Forms;

namespace EasyCsv.Tests.Files
{
    public class EasyCsvFileTests
    {
        private const string SingleLineCsv = "header1,header2\nvalue1,value2";
        [Fact]
        public async Task TryReadFormFileAsync_ReadsFileContentSuccessfully()
        {
            // Arrange
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(SingleLineCsv));
            var formFileMock = new Mock<IFormFile>();
            formFileMock.Setup(file => file.Length).Returns(stream.Length);
            formFileMock.Setup(file => file.CopyToAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
                .Callback((Stream targetStream, CancellationToken cancellationToken) => stream.CopyTo(targetStream));

            var easyCsv = new Core.EasyCsv();

            // Act
            var result = await easyCsv.TryReadFileAsync(formFileMock.Object, 1024 * 1024 * 15);

            // Assert
            Assert.True(result);
            Assert.Single(easyCsv.CsvContent);
        }

        [Fact]
        public async Task TryReadBrowserFileAsync_ReadsFileContentSuccessfully()
        {
            // Arrange
            var fileContent = "header1,header2\nvalue1,value2";
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(fileContent));
            var browserFileMock = new Mock<IBrowserFile>();
            browserFileMock.Setup(file => file.Size).Returns(stream.Length);
            browserFileMock.Setup(file => file.OpenReadStream(It.IsAny<long>(), It.IsAny<CancellationToken>())).Returns(stream);
            

            var easyCsv = new Core.EasyCsv();

            // Act
            var result = await easyCsv.TryReadFileAsync(browserFileMock.Object, 1024 * 1024 * 15);

            // Assert
            Assert.True(result);
            Assert.Single(easyCsv.CsvContent!);
        }
    }
}