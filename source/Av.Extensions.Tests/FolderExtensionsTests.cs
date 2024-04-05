// <copyright file="FolderExtensionsTests.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

using Av.Store;

namespace Av.Extensions.Tests;

/// <summary>
/// Tests for the <see cref="FolderExtensions"/> class.
/// </summary>
public class FolderExtensionsTests
{
    [Fact]
    public async Task Ingest_WithFiles_ProcessedAsExpected()
    {
        // Arrange
        var ogDir = new DirectoryInfo("Samples");
        const string secureName = "1bcedf85fab4eae955a6444ee7b2d70be3b5fe02bdebaecd433828f9731630da.flv";
        var sourceDir = ogDir.CreateSubdirectory(Guid.NewGuid().ToString());
        var targetDir = ogDir.CreateSubdirectory(Guid.NewGuid().ToString());
        File.Copy($"{ogDir}/non-media.txt", $"{sourceDir}/non-media.txt");
        File.Copy($"{ogDir}/sample.flv", $"{sourceDir}/sample.flv");
        File.Copy($"{ogDir}/{secureName}", $"{sourceDir}/{secureName}");
        var expected = new BulkItemResponse { Total = 3, Processed = 1, Skipped = 1, Unmatched = 1 };

        // Act
        var result = await sourceDir.Ingest([9, 0, 2, 1, 0], targetDir.FullName);

        // Assert
        result.Should().Be(expected);
        sourceDir.Delete(true);
        targetDir.Delete(true);
    }
}
