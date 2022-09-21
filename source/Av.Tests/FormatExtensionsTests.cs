namespace Av.Tests;

/// <summary>
/// Tests for the <see cref="FormatExtensions"/>.
/// </summary>
public class FormatExtensionsTests
{
    [Theory]
    [InlineData(1L, "1b")]
    [InlineData(1L * 1024, "1kb")]
    [InlineData(1L * 1024 * 1024, "1mb")]
    [InlineData(1L * 1024 * 1024 * 1024, "1gb")]
    [InlineData(1L * 1024 * 1024 * 1024 * 1024, "1tb")]
    public void FormatSize_VaryingSize_ReturnsExpected(long bytes, string expected)
    {
        // Arrange & Act
        var result = bytes.FormatSize();

        // Assert
        result.Should().Be(expected);
    }
}
