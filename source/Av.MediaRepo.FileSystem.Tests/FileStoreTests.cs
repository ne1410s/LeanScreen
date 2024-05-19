// <copyright file="FileStoreTests.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace Av.MediaRepo.FileSystem.Tests;

using Av.MediaRepo.FileSystem;
using CryptoStream.IO;

/// <summary>
/// Tests for the <see cref="FileStore"/> class.
/// </summary>
public class FileStoreTests
{
    [Fact]
    public async Task AddMedia_IsSecure_DoesNotThrow()
    {
        // Arrange
        var file = new string('0', 64) + ".4c26c413c2";
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

    [Fact]
    public async Task NextUncapped_HasCounterpart_NotIncluded()
    {
        // Arrange
        var dir = Guid.NewGuid().ToString();
        new DirectoryInfo(dir).Create();
        var fi = new FileInfo($"{dir}/{Guid.NewGuid()}.avi");
        await File.WriteAllTextAsync(fi.FullName, "hello");
        fi.EncryptInSitu([9, 0, 2, 1, 0]);
        var capExt = fi.ToSecureExtension(".avi");
        var capName = $"{fi.DirectoryName}/{fi.Name[..12]}.0123" + capExt;
        await File.WriteAllTextAsync(capName, "world");
        var sut = new FileStore(fi.DirectoryName!);

        // Act
        var todos = await sut.NextUncapped();

        // Assert
        todos.Should().BeEmpty();
        Directory.Delete(fi.DirectoryName!, true);
    }
}