using Av.Models;

namespace Av.Tests.Models;

/// <summary>
/// Tests for the <see cref="MediaTypeInfo"/> class.
/// </summary>
public class MediaTypeInfoTests
{
    [Theory]
    [InlineData(".7z", MediaTypes.Archive, "application/x-7z-compressed")]
    [InlineData(".gz", MediaTypes.Archive, "application/gzip")]
    [InlineData(".jar", MediaTypes.Archive, "application/java-archive")]
    [InlineData(".rar", MediaTypes.Archive, "application/vnd.rar")]
    [InlineData(".tar", MediaTypes.Archive, "application/x-tar")]
    [InlineData(".zip", MediaTypes.Archive, "application/zip")]
    [InlineData(".bmp", MediaTypes.Image, "image/bmp")]
    [InlineData(".gif", MediaTypes.Image, "image/gif")]
    [InlineData(".jpe", MediaTypes.Image, "image/jpeg")]
    [InlineData(".jpeg", MediaTypes.Image, "image/jpeg")]
    [InlineData(".jpg", MediaTypes.Image, "image/jpeg")]
    [InlineData(".png", MediaTypes.Image, "image/png")]
    [InlineData(".tif", MediaTypes.Image, "image/tiff")]
    [InlineData(".tiff", MediaTypes.Image, "image/tiff")]
    [InlineData(".webp", MediaTypes.Image, "image/webp")]
    [InlineData(".aac", MediaTypes.Audio, "audio/aac")]
    [InlineData(".m4a", MediaTypes.Audio, "audio/m4a")]
    [InlineData(".mp3", MediaTypes.Audio, "audio/mpeg")]
    [InlineData(".oga", MediaTypes.Audio, "audio/ogg")]
    [InlineData(".wav", MediaTypes.Audio, "audio/wav")]
    [InlineData(".weba", MediaTypes.Audio, "audio/webm")]
    [InlineData(".3g2", MediaTypes.Video, "video/3gpp2")]
    [InlineData(".3gp", MediaTypes.Video, "video/3gpp")]
    [InlineData(".avi", MediaTypes.Video, "video/x-msvideo")]
    [InlineData(".flv", MediaTypes.Video, "video/x-flv")]
    [InlineData(".m2v", MediaTypes.Video, "video/mpeg")]
    [InlineData(".m4v", MediaTypes.Video, "video/x-m4v")]
    [InlineData(".mkv", MediaTypes.Video, "video/x-matroska")]
    [InlineData(".mov", MediaTypes.Video, "video/quicktime")]
    [InlineData(".mp4", MediaTypes.Video, "video/mp4")]
    [InlineData(".mpeg", MediaTypes.Video, "video/mpeg")]
    [InlineData(".mpg", MediaTypes.Video, "video/mpeg")]
    [InlineData(".mts", MediaTypes.Video, "video/mp2t")]
    [InlineData(".swf", MediaTypes.Video, "application/x-shockwave-flash")]
    [InlineData(".ts", MediaTypes.Video, "video/mp2t")]
    [InlineData(".ogg", MediaTypes.Video, "video/ogg")]
    [InlineData(".ogv", MediaTypes.Video, "video/ogg")]
    [InlineData(".vob", MediaTypes.Video, "video/x-ms-vob")]
    [InlineData(".webm", MediaTypes.Video, "video/webm")]
    [InlineData(".wmv", MediaTypes.Video, "video/x-ms-wmv")]
    public void Get_ValidExtension_ReturnsInfo(string extension, MediaTypes expectedType, string expectedMime)
    {
        // Arrange
        var expected = new MediaTypeInfo(expectedType, expectedMime);

        // Act
        var result = MediaTypeInfo.Get(extension);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(".zip")]
    [InlineData("ZIp")]
    public void Get_VarietyOfFormats_ReturnsInfo(string extension)
    {
        // Arrange & Act
        var result = MediaTypeInfo.Get(extension);

        // Assert
        result.MediaType.Should().Be(MediaTypes.Archive);
        result.MimeType.Should().Be("application/zip");
    }

    [Theory]
    [InlineData(".")]
    [InlineData("")]
    [InlineData(null)]
    [InlineData(" ")]
    [InlineData("fakerzz")]
    [InlineData("zip.")]
    public void Get_NotFound_ReturnsNull(string extension)
    {
        // Arrange
        var result = MediaTypeInfo.Get(extension);

        // Assert
        result.MimeType.Should().Be(null);
    }
}
