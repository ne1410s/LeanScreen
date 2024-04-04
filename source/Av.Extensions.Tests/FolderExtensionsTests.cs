// <copyright file="FolderExtensionsTests.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace Av.Extensions.Tests;

/// <summary>
/// Tests for the <see cref="FolderExtensions"/> class.
/// </summary>
public class FolderExtensionsTests
{
    [Fact]
    public async Task Ingest_Files_MovedOk()
    {
        // Arrange
        var ogFile = new FileInfo(Path.Combine("Samples", "sample.flv"));
        var sourceDir = ogFile.Directory!.CreateSubdirectory(Guid.NewGuid().ToString());
        var targetDir = ogFile.Directory!.CreateSubdirectory(Guid.NewGuid().ToString());
        ogFile.CopyTo(Path.Combine(sourceDir.FullName, ogFile.Name), true);

        // Act
        var result = await sourceDir.Ingest([9, 0, 2, 1, 0], targetDir.FullName);

        // Assert
        result.Processed.Should().Be(1);
        sourceDir.Delete(true);
        targetDir.Delete(true);
    }
}
