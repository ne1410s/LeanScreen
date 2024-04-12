// <copyright file="FileStoreTests.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace Av.MediaRepo.FileSystem.Tests;

using Av.MediaRepo.FileSystem;

/// <summary>
/// Tests for the <see cref="FileStore"/> class.
/// </summary>
public class FileStoreTests
{
    [Fact]
    public async Task AddMedia_IsSecure_DoesNotThrow()
    {
        // Arrange
        var file = new string('0', 64) + ".jpg";
        var sut = new FileStore("dir");

        // Act
        var act = () => sut.AddMedia(new MemoryStream(), file);

        // Assert
        await act.Should().NotThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task AddMedia_NotSecure_ThrowsException()
    {
        // Arrange
        var sut = new FileStore("dir");

        // Act
        var act = () => sut.AddMedia(new MemoryStream(), "123");

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Source is not secure.");
    }

    [Fact]
    public async Task AddCaps_SourceMatch_ThrowsSomeOtherError()
    {
        // Arrange
        var sut = new FileStore("dir");

        // Act
        var act = () => sut.AddCaps(new MemoryStream(), "123456789012", "123456789012");

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Source is not secure.");
    }

    [Fact]
    public async Task AddCaps_SourceMismatch_ThrowsException()
    {
        // Arrange
        var sut = new FileStore("dir");

        // Act
        var act = () => sut.AddCaps(new MemoryStream(), "123", "123456789012");

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Source name is not consistent with parent.");
    }
}