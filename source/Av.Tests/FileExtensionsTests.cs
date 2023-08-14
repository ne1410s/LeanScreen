// <copyright file="FileExtensionsTests.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace Av.Tests;

using Av.Models;

/// <summary>
/// Tests for the <see cref="FileExtensions"/>.
/// </summary>
public class FileExtensionsTests
{
    [Fact]
    public void GetMediaTypeInfo_WithJpg_ReturnsExpected()
    {
        // Arrange
        var fi = new FileInfo("test.jpg");
        var expected = new MediaTypeInfo(MediaTypes.Image, "image/jpeg");

        // Act
        var result = fi.GetMediaTypeInfo();

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void GetMediaTypeInfo_NullFile_ThrowsExpected()
    {
        // Arrange
        var fi = (FileInfo)null!;

        // Act
        var act = fi.GetMediaTypeInfo;

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void EnumerateMedia_NullDirectory_ThrowsExpected()
    {
        // Arrange
        var fi = (DirectoryInfo)null!;

        // Act
        var act = () => fi.EnumerateMedia(MediaTypes.AnyMedia);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Theory]
    [InlineData(MediaTypes.Audio, 0)]
    [InlineData(MediaTypes.Image, 2)]
    [InlineData(MediaTypes.Video, 1)]
    [InlineData(MediaTypes.AnyMedia, 3)]
    [InlineData(MediaTypes.NonMedia, 1)]
    public void EnumerateMedia_VaryingMediaType_CountExpected(MediaTypes types, int expectedCount)
    {
        // Arrange
        var di = new DirectoryInfo("Samples");

        // Act
        var media = di.EnumerateMedia(types, recurse: true);

        // Assert
        media.Count().Should().Be(expectedCount);
    }

    [Theory]
    [InlineData(null, 3)]
    [InlineData(false, 2)]
    [InlineData(true, 1)]
    public void EnumerateMedia_VaryingSecureFlag_CountExpected(bool? secure, int expectedCount)
    {
        // Arrange
        var di = new DirectoryInfo("Samples");

        // Act
        var media = di.EnumerateMedia(MediaTypes.AnyMedia, secure, true);

        // Assert
        media.Count().Should().Be(expectedCount);
    }

    [Theory]
    [InlineData(false, 1)]
    [InlineData(true, 3)]
    public void EnumerateMedia_VaryingRecurseFlag_CountExpected(bool recurse, int expectedCount)
    {
        // Arrange
        var di = new DirectoryInfo("Samples");

        // Act
        var media = di.EnumerateMedia(MediaTypes.AnyMedia, recurse: recurse);

        // Assert
        media.Count().Should().Be(expectedCount);
    }

    [Fact]
    public void EncryptMediaTo_NullTarget_ThrowsException()
    {
        // Arrange
        var di = new DirectoryInfo("Samples");

        // Act
        var act = () => di.EncryptMediaTo(null, Array.Empty<byte>());

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .Which.ParamName.Should().Be("target");
    }

    [Fact]
    public void EncryptMediaTo_WithSortLength_GeneratesExpected()
    {
        // Arrange
        var source = new DirectoryInfo(Guid.NewGuid().ToString());
        var target = new DirectoryInfo(Guid.NewGuid().ToString());
        source.Create();
        File.Copy(Path.Combine("Samples", "blue-pixel.png"), Path.Combine(source.Name, "file.png"));
        const string expectedName = "6b0ea016cd3a043a496502126ad46ba80448b207a642831e19166b64de81ec13.png";

        // Act
        source.EncryptMediaTo(target, new byte[] { 1, 2, 3, 4 }, sortFolderLength: 3);
        var exists = File.Exists(Path.Combine(target.Name, "6b0", expectedName));

        // Assert
        exists.Should().BeTrue();
    }

    [Fact]
    public void EncryptMediaTo_ZeroSortLength_GeneratesExpected()
    {
        // Arrange
        var source = new DirectoryInfo(Guid.NewGuid().ToString());
        var target = new DirectoryInfo(Guid.NewGuid().ToString());
        source.Create();
        File.Copy(Path.Combine("Samples", "blue-pixel.png"), Path.Combine(source.Name, "file.png"));
        const string expectedName = "6b0ea016cd3a043a496502126ad46ba80448b207a642831e19166b64de81ec13.png";

        // Act
        source.EncryptMediaTo(target, new byte[] { 1, 2, 3, 4 }, sortFolderLength: 0);
        var exists = File.Exists(Path.Combine(target.Name, expectedName));

        // Assert
        exists.Should().BeTrue();
    }
}
