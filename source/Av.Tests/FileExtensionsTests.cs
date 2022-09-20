using Av.Models;

namespace Av.Tests;

/// <summary>
/// Tests for the <see cref="FileExtensions"/>.
/// </summary>
public class FileExtensionsTests
{
    [Fact]
    public void GetMediaTypeInfo_WithJpeg_ReturnsExpected()
    {
        // Arrange
        var fi = new FileInfo("test.jpg");

        // Act
        var result = fi.GetMediaTypeInfo();

        // Assert
        result.Should().Be(MediaTypeInfo.Get("jpg"));
    }
}
