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
        const string secureName = "c12a3419943d6ceb89c41ce7cd4fe1ff75b991cc3cb01a31a13b08693c5dc63d.e4e4742e63";
        const string pregenName = "5e84bf533440"
            + ".fb62352ee50d77e90b9d4c59f92263b576756148e1cee33b8ad338741b2af7b4.63e74026ac";
        var sourceDir = ogDir.CreateSubdirectory(Guid.NewGuid().ToString());
        var targetDir = ogDir.CreateSubdirectory(Guid.NewGuid().ToString());
        targetDir.CreateSubdirectory("5e");
        File.Copy($"{ogDir}/non-media.txt", $"{sourceDir}/non-media.txt");
        File.Copy($"{ogDir}/sample.flv", $"{sourceDir}/sample.flv");
        File.Copy($"{ogDir}/1.mkv", $"{sourceDir}/1.mkv");
        File.Copy($"{ogDir}/{secureName}", $"{sourceDir}/{secureName}");
        File.Copy($"{ogDir}/{pregenName}", $"{targetDir}/5e/{pregenName}");
        var expected = new BulkResponse(4) { Processed = 2, Skipped = 1, Unmatched = 1 };
        var expectSaveTo = $"{targetDir}/5e/"
            + "5e84bf533440c477c441fc829d173c0286ccf8c155b6a9aff325de4564f63c26.b97a77d7e2";

        // Act
        var result = await sourceDir.Ingest([9, 0, 2, 1, 0], targetDir.FullName);

        // Assert
        result.Should().Be(expected);
        new FileInfo(expectSaveTo).Length.Should().NotBe(0);
        new FileInfo($"{sourceDir}/sample.flv").Exists.Should().BeFalse();
        sourceDir.Delete(true);
        targetDir.Delete(true);
    }

    [Fact]
    public async Task Ingest_NewlySecuredFile_CapsExpected()
    {
        // Arrange
        var ogDir = new DirectoryInfo("Samples");
        var sourceDir = ogDir.CreateSubdirectory(Guid.NewGuid().ToString());
        var targetDir = ogDir.CreateSubdirectory(Guid.NewGuid().ToString());
        File.Copy($"{ogDir}/sample.flv", $"{sourceDir}/sample.flv");
        const string name = "c12a3419943d.ceff13d13fc86ee668a86a90095f6be245d0b67ca469febafef902dee03fde0e.06984d1654";

        // Act
        await sourceDir.Ingest([9, 0, 2, 1, 0], targetDir.FullName, applySnap: true);

        // Assert
        new FileInfo($"{targetDir}/c1/{name}").Exists.Should().BeTrue();
        sourceDir.Delete(true);
        targetDir.Delete(true);
    }

    [Fact]
    public async Task Ingest_PlainSourceWithUncappedAnalogue_CapsExpected()
    {
        // Arrange
        var ogDir = new DirectoryInfo("Samples");
        const string uncapped = "c12a3419943d6ceb89c41ce7cd4fe1ff75b991cc3cb01a31a13b08693c5dc63d.e4e4742e63";
        var sourceDir = ogDir.CreateSubdirectory(Guid.NewGuid().ToString());
        var targetDir = ogDir.CreateSubdirectory(Guid.NewGuid().ToString());
        targetDir.CreateSubdirectory("c1");
        File.Copy($"{ogDir}/sample.flv", $"{sourceDir}/sample.flv");
        File.Copy($"{ogDir}/{uncapped}", $"{targetDir}/c1/{uncapped}");
        const string name = "c12a3419943d.ceff13d13fc86ee668a86a90095f6be245d0b67ca469febafef902dee03fde0e.06984d1654";

        // Act
        await sourceDir.Ingest([9, 0, 2, 1, 0], targetDir.FullName, applySnap: true);

        // Assert
        new FileInfo($"{targetDir}/c1/{name}").Exists.Should().BeTrue();
        sourceDir.Delete(true);
        targetDir.Delete(true);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Ingest_VaryingPurge_OnlyPurgeRemovedNonPertinentFile(bool purge)
    {
        // Arrange
        var ogDir = new DirectoryInfo("Samples");
        var sourceDir = ogDir.CreateSubdirectory(Guid.NewGuid().ToString());
        var targetDir = ogDir.CreateSubdirectory(Guid.NewGuid().ToString());
        var nonMediaPath = $"{sourceDir}/1.txt";
        await File.WriteAllTextAsync(nonMediaPath, "non-perinent!");

        // Act
        await sourceDir.Ingest([], targetDir.FullName, purgeNonMedia: purge);

        // Assert
        new FileInfo(nonMediaPath).Exists.Should().Be(!purge);
    }

    [Fact]
    public async Task Ingest_WithProgress_InvokesAction()
    {
        // Arrange
        var mockProgress = new Mock<IProgress<double>>();
        var ogDir = new DirectoryInfo("Samples");
        const string storeFile = "5e84bf533440c477c441fc829d173c0286ccf8c155b6a9aff325de4564f63c26.b97a77d7e2";
        var sourceDir = ogDir.CreateSubdirectory(Guid.NewGuid().ToString());
        var targetDir = ogDir.CreateSubdirectory(Guid.NewGuid().ToString());
        targetDir.CreateSubdirectory("5e");
        File.Copy($"{ogDir}/1.mkv", $"{sourceDir}/1.mkv");
        File.Copy($"{ogDir}/{storeFile}", $"{targetDir}/5e/{storeFile}");
        File.Copy($"{ogDir}/non-media.txt", $"{sourceDir}/non-media.txt");
        var expected = new BulkResponse(2) { Processed = 1, Unmatched = 1 };

        // Act
        var result = await sourceDir.Ingest([], targetDir.FullName, onProgress: mockProgress.Object);

        // Assert
        result.Should().Be(expected);
        mockProgress.Verify(m => m.Report(0));
        mockProgress.Verify(m => m.Report(50));
        mockProgress.Verify(m => m.Report(100), Times.Exactly(2));
    }

    [Fact]
    public async Task ApplyCaps_WithProgress_InvokesAction()
    {
        // Arrange
        var mockProgress = new Mock<IProgress<double>>();
        var ogDir = new DirectoryInfo("Samples");
        const string storeFile1 = "c12a3419943d6ceb89c41ce7cd4fe1ff75b991cc3cb01a31a13b08693c5dc63d.e4e4742e63";
        const string storeFile2 = "5e84bf533440c477c441fc829d173c0286ccf8c155b6a9aff325de4564f63c26.b97a77d7e2";
        const string storeFile3 = "c49fc2afcf45544db942a83817f99b625d40cd30ec46a044b68d79bc995ddaf1.8a2216a3b3";

        var targetDir = ogDir.CreateSubdirectory(Guid.NewGuid().ToString());
        targetDir.CreateSubdirectory("c1");
        targetDir.CreateSubdirectory("5e");
        targetDir.CreateSubdirectory("c4");
        File.Copy($"{ogDir}/{storeFile1}", $"{targetDir}/c1/{storeFile1}");
        File.Copy($"{ogDir}/{storeFile2}", $"{targetDir}/5e/{storeFile2}");
        File.Copy($"{ogDir}/{storeFile3}", $"{targetDir}/c4/{storeFile3}");

        // Act
        await BulkMediaUtils.ApplyCaps([9, 0, 2, 1, 0], targetDir.FullName, onProgress: mockProgress.Object);

        // Assert
        mockProgress.Verify(m => m.Report(0));
        mockProgress.Verify(m => m.Report(100 / 3d));
        mockProgress.Verify(m => m.Report(200 / 3d));
        mockProgress.Verify(m => m.Report(100), Times.Exactly(2));
        targetDir.Delete(true);
    }

    [Fact]
    public async Task ApplyCaps_HasSomeWorkToDo_AppliesCaps()
    {
        // Arrange
        var ogDir = new DirectoryInfo("Samples");
        const string storeFile1 = "c12a3419943d6ceb89c41ce7cd4fe1ff75b991cc3cb01a31a13b08693c5dc63d.e4e4742e63";
        const string storeFile2 = "5e84bf533440c477c441fc829d173c0286ccf8c155b6a9aff325de4564f63c26.b97a77d7e2";
        const string storeFile3 = "c49fc2afcf45544db942a83817f99b625d40cd30ec46a044b68d79bc995ddaf1.8a2216a3b3";
        const string pregenName = "5e84bf533440" +
            ".fb62352ee50d77e90b9d4c59f92263b576756148e1cee33b8ad338741b2af7b4.63e74026ac";
        var targetDir = ogDir.CreateSubdirectory(Guid.NewGuid().ToString());
        targetDir.CreateSubdirectory("c1");
        targetDir.CreateSubdirectory("5e");
        targetDir.CreateSubdirectory("c4");
        File.Copy($"{ogDir}/{storeFile1}", $"{targetDir}/c1/{storeFile1}");
        File.Copy($"{ogDir}/{storeFile2}", $"{targetDir}/5e/{storeFile2}");
        File.Copy($"{ogDir}/{storeFile3}", $"{targetDir}/c4/{storeFile3}");
        File.Copy($"{ogDir}/{pregenName}", $"{targetDir}/5e/{pregenName}");
        const string expectedName = "c49fc2afcf45"
            + ".4d05057d335e0a9fed0f8542e263f5018905250ee30bf0911ec75498a6352b18.8780a82eb8";

        // Act
        await BulkMediaUtils.ApplyCaps([9, 0, 2, 1, 0], targetDir.FullName);

        // Assert
        new FileInfo($"{targetDir}/c4/{expectedName}").Exists.Should().Be(true);
        targetDir.Delete(true);
    }

    [Fact]
    public async Task ApplyCaps_MaxLimiting_MaxApplied()
    {
        // Arrange
        var ogDir = new DirectoryInfo("Samples");
        const string storeFile1 = "c12a3419943d6ceb89c41ce7cd4fe1ff75b991cc3cb01a31a13b08693c5dc63d.e4e4742e63";
        const string storeFile2 = "5e84bf533440c477c441fc829d173c0286ccf8c155b6a9aff325de4564f63c26.b97a77d7e2";
        const string storeFile3 = "c49fc2afcf45544db942a83817f99b625d40cd30ec46a044b68d79bc995ddaf1.8a2216a3b3";
        const string pregenName = "5e84bf533440" +
            ".fb62352ee50d77e90b9d4c59f92263b576756148e1cee33b8ad338741b2af7b4.63e74026ac";
        var targetDir = ogDir.CreateSubdirectory(Guid.NewGuid().ToString());
        targetDir.CreateSubdirectory("c1");
        targetDir.CreateSubdirectory("5e");
        targetDir.CreateSubdirectory("c4");
        File.Copy($"{ogDir}/{storeFile1}", $"{targetDir}/c1/{storeFile1}");
        File.Copy($"{ogDir}/{storeFile2}", $"{targetDir}/5e/{storeFile2}");
        File.Copy($"{ogDir}/{storeFile3}", $"{targetDir}/c4/{storeFile3}");
        File.Copy($"{ogDir}/{pregenName}", $"{targetDir}/5e/{pregenName}");

        // Act
        var result = await BulkMediaUtils.ApplyCaps([9, 0, 2, 1, 0], targetDir.FullName, max: 1);

        // Assert
        result.Should().Be(1);
        targetDir.Delete(true);
    }

    [Theory]
    [InlineData("blob", "Value cannot be null.*")]
    [InlineData("literally-anything-else", "*empty*")]
    public void GetRepo_VaryingParam_ThrowsExpected(string requestName, string expectedError)
    {
        // Arrange & Act
        var act = () => BulkMediaUtils.GetRepo(requestName, string.Empty);

        // Assert
        act.Should().Throw<Exception>().WithMessage(expectedError);
    }
}
