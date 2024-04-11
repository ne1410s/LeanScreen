// <copyright file="BulkMediaUtilsTests.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace Av.Extensions.Tests;

using Av.BulkProcess;
using Moq;

/// <summary>
/// Tests for the <see cref="BulkMediaUtils"/> class.
/// </summary>
public class BulkMediaUtilsTests
{
    [Fact]
    public async Task Ingest_WithFiles_ProcessedAsExpected()
    {
        // Arrange
        var ogDir = new DirectoryInfo("Samples");
        const string secureName = "1bcedf85fab4eae955a6444ee7b2d70be3b5fe02bdebaecd433828f9731630da.flv";
        const string pregenName = "7ee3322921c9.227c443d7e12ad1402229ed2c2492d9547ebee2332e7cc4c8a6a4a3d4c156bc3.jpg";
        var sourceDir = ogDir.CreateSubdirectory(Guid.NewGuid().ToString());
        var targetDir = ogDir.CreateSubdirectory(Guid.NewGuid().ToString());
        targetDir.CreateSubdirectory("7e");
        File.Copy($"{ogDir}/non-media.txt", $"{sourceDir}/non-media.txt");
        File.Copy($"{ogDir}/sample.flv", $"{sourceDir}/sample.flv");
        File.Copy($"{ogDir}/1.mkv", $"{sourceDir}/1.mkv");
        File.Copy($"{ogDir}/{secureName}", $"{sourceDir}/{secureName}");
        File.Copy($"{ogDir}/{pregenName}", $"{targetDir}/7e/{pregenName}");
        var expected = new BulkResponse(4) { Processed = 2, Skipped = 1, Unmatched = 1 };

        // Act
        var result = await sourceDir.Ingest([9, 0, 2, 1, 0], targetDir.FullName);

        // Assert
        result.Should().Be(expected);
        sourceDir.Delete(true);
        targetDir.Delete(true);
    }

    [Fact]
    public async Task Ingest_WithProgress_InvokesAction()
    {
        // Arrange
        var mockProgress = new Mock<IProgress<double>>();
        var ogDir = new DirectoryInfo("Samples");
        const string storeFile = "7ee3322921c9880a3fffc5e55b31521dfbf3c07e634736bbbb2ea0a8de6deec3.mkv";
        var sourceDir = ogDir.CreateSubdirectory(Guid.NewGuid().ToString());
        var targetDir = ogDir.CreateSubdirectory(Guid.NewGuid().ToString());
        targetDir.CreateSubdirectory("7e");
        File.Copy($"{ogDir}/1.mkv", $"{sourceDir}/1.mkv");
        File.Copy($"{ogDir}/{storeFile}", $"{targetDir}/7e/{storeFile}");

        // Act
        await sourceDir.Ingest([], targetDir.FullName, onProgress: mockProgress.Object);

        // Assert
        mockProgress.Verify(m => m.Report(It.IsAny<double>()));
    }

    [Fact]
    public async Task ApplyCaps_WithProgress_InvokesAction()
    {
        // Arrange
        var mockProgress = new Mock<IProgress<double>>();
        var ogDir = new DirectoryInfo("Samples");
        const string storeFile1 = "1bcedf85fab4eae955a6444ee7b2d70be3b5fe02bdebaecd433828f9731630da.flv";
        var targetDir = ogDir.CreateSubdirectory(Guid.NewGuid().ToString());
        targetDir.CreateSubdirectory("1b");
        File.Copy($"{ogDir}/{storeFile1}", $"{targetDir}/1b/{storeFile1}");

        // Act
        await BulkMediaUtils.ApplyCaps([9, 0, 2, 1, 0], targetDir.FullName, onProgress: mockProgress.Object);

        // Assert
        mockProgress.Verify(m => m.Report(It.IsAny<double>()));
        targetDir.Delete(true);
    }

    [Theory]
    [InlineData(100, 2)]
    [InlineData(1, 1)]
    public async Task ApplyCaps_HasSomeWorkToDo_AppliesCaps(int max, int expected)
    {
        // Arrange
        var ogDir = new DirectoryInfo("Samples");
        const string storeFile1 = "1bcedf85fab4eae955a6444ee7b2d70be3b5fe02bdebaecd433828f9731630da.flv";
        const string storeFile2 = "7ee3322921c9880a3fffc5e55b31521dfbf3c07e634736bbbb2ea0a8de6deec3.mkv";
        const string storeFile3 = "38595346adffe0060eb052c38a636ec0149d5bfb1ef42ea64cd76e83f91af51b.flv";
        const string pregenName = "7ee3322921c9.227c443d7e12ad1402229ed2c2492d9547ebee2332e7cc4c8a6a4a3d4c156bc3.jpg";
        var targetDir = ogDir.CreateSubdirectory(Guid.NewGuid().ToString());
        targetDir.CreateSubdirectory("1b");
        targetDir.CreateSubdirectory("7e");
        targetDir.CreateSubdirectory("38");
        File.Copy($"{ogDir}/{storeFile1}", $"{targetDir}/1b/{storeFile1}");
        File.Copy($"{ogDir}/{storeFile2}", $"{targetDir}/7e/{storeFile2}");
        File.Copy($"{ogDir}/{storeFile3}", $"{targetDir}/38/{storeFile3}");
        File.Copy($"{ogDir}/{pregenName}", $"{targetDir}/7e/{pregenName}");

        // Act
        var result = await BulkMediaUtils.ApplyCaps([9, 0, 2, 1, 0], targetDir.FullName, max: max);

        // Assert
        result.Should().Be(expected);
        targetDir.Delete(true);
    }

    [Theory]
    [InlineData("blob", "Value cannot be null.*")]
    [InlineData("literally-anything-else", "The path is empty.*")]
    public void GetRepo_VaryingParam_ThrowsExpected(string requestName, string expectedError)
    {
        // Arrange & Act
        var act = () => BulkMediaUtils.GetRepo(requestName, string.Empty);

        // Assert
        act.Should().Throw<Exception>().WithMessage(expectedError);
    }
}
