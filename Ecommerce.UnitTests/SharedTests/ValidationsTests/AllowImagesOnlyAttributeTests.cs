using System.Net.Mime;
using Ecommerce.Shared.Validations;
using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;

namespace Ecommerce.UnitTests.SharedTests.ValidationsTests
{
    public abstract class AllowImagesOnlyAttributeTests
    {
        protected AllowImagesOnlyAttribute ServiceUnderTest { get; }
        public AllowImagesOnlyAttributeTests()
        {
            ServiceUnderTest = new AllowImagesOnlyAttribute();
        }

        public class IsValid : AllowImagesOnlyAttributeTests
        {
            [Theory]
            [InlineData(MediaTypeNames.Application.Json)]
            [InlineData(MediaTypeNames.Application.Pdf)]
            [InlineData(MediaTypeNames.Text.Html)]
            [InlineData(null)]
            public void Should_return_false_if_contentType_is_null_or_invalid(string contentType)
            {
                // Arrange
                var fileMock = new Mock<IFormFile>();
                fileMock.Setup(x => x.ContentType).Returns(contentType);

                // Act
                var result = ServiceUnderTest.IsValid(fileMock.Object);

                // Assert
                Assert.False(result);
            }

            [Theory]
            [InlineData(MediaTypeNames.Image.Gif)]
            [InlineData(MediaTypeNames.Image.Jpeg)]
            [InlineData(MediaTypeNames.Image.Tiff)]
            public void Should_return_true_if_contentType_is_valid(string contentType)
            {
                // Arrange
                var fileMock = new Mock<IFormFile>();
                fileMock.Setup(x => x.ContentType).Returns(contentType);

                // Act
                var result = ServiceUnderTest.IsValid(fileMock.Object);

                // Assert
                Assert.True(result);
            }
            [Fact]
            public void Should_return_true_if_file_is_null()
            {
                // Act
                var result = ServiceUnderTest.IsValid(null);
                // Assert
                Assert.True(result);
            }
        }
    }
}