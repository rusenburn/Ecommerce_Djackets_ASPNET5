using System.ComponentModel.DataAnnotations;
using Ecommerce.Shared.Validations;
using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;

namespace Ecommerce.UnitTests.SharedTests.ValidationsTests
{
    public abstract class MaxFileSizeTests
    {
        protected MaxFileSizeAttribute ServiceUnderTest { get; }
        protected long MaxFileSize { get; }
        public MaxFileSizeTests()
        {
            MaxFileSize = 100L;
            ServiceUnderTest = new MaxFileSizeAttribute(MaxFileSize);
        }

        public class IsValid : MaxFileSizeTests
        {
            [Fact]
            public void Should_return_ValidationSuccess_if_null_is_passed()
            {

                // Act
                var result = ServiceUnderTest.IsValid(null);

                // Assert
                Assert.True(result);
            }

            [Fact]
            public void Should_return_False_if_file_size_is_bigger_than_maximum()
            {
                // Arrange
                var fileMock = new Mock<IFormFile>();
                fileMock.Setup(x=>x.Length).Returns(MaxFileSize + 1);

                // Act
                var result = ServiceUnderTest.IsValid(fileMock.Object);

                // Assert
                Assert.False(result);
            }

            [Fact]
            public void Should_return_true_if_file_size_is_smaller_than_maximum()
            {
                // Arrange
                var fileMock = new Mock<IFormFile>();
                fileMock.Setup(x=>x.Length).Returns(MaxFileSize);

                // Act
                var result = ServiceUnderTest.IsValid(fileMock.Object);

                // Assert
                Assert.True(result);
            }
        }
    }
}