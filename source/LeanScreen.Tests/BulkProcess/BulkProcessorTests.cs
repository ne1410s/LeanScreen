// <copyright file="BulkProcessorTests.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace LeanScreen.Tests.BulkProcess;

using LeanScreen.BulkProcess;
using LeanScreen.Common;
using LeanScreen.MediaRepo;
using LeanScreen.Services;
using LeanScreen.Tests.Samples;

/// <summary>
/// Tests for the <see cref="BulkProcessor"/> class.
/// </summary>
public class BulkProcessorTests
{
    [Fact]
    public async Task IngestAsync_WhenCalled_DeletesProcessed()
    {
        // Arrange
        var sut = GetSut(out _);
        var di = TestHelper.CopySamples();
        const string sourceImage = "blue-pixel.png";
        const string derivative = "3122188888*";

        // Act
        _ = await sut.IngestAsync([], di, false, false, false, false);

        // Assert
        di.GetFiles(sourceImage).Length.ShouldBe(0);
        di.GetFiles(derivative).Length.ShouldBe(0);
        di.Delete(true);
    }

    [Fact]
    public async Task IngestAsync_ExistingVid_AddsCaps()
    {
        // Arrange
        var sut = GetSut(out var mocks);
        var di = TestHelper.CopySamples();
        var expected = new BulkResponse(4) { Unmatched = 1, Processed = 3 };
        _ = mocks.MockRepo
            .Setup(m => m.FindAsync(It.IsAny<string>()))
            .ReturnsAsync(["101fe0480635e03536b17760cb8526f6b039f28f228140eea5ce4a3d7653a15c.47f14f5297"]);

        // Act
        var result = await sut.IngestAsync([], di, true, false, true, true);

        // Assert
        di.Delete(true);
        result.ShouldBe(expected);
        mocks.MockRepo.Verify(
            m => m.AddCaps(
                It.IsAny<Stream>(),
                "101fe0480635.5650a7421836ebd7b0d5a4de846724e8416cdd1c5368d20fdee612741ba5156e.65ffb9eb00",
                "101fe0480635e03536b17760cb8526f6b039f28f228140eea5ce4a3d7653a15c.47f14f5297"));
    }

    private static BulkProcessor GetSut(out BagOfMocks mocks)
    {
        Size2D size;
        mocks = new(new Mock<ISnapService>(), new Mock<IMediaRepo>());
        _ = mocks.MockSnapper
            .Setup(m => m.Collate(
                It.IsAny<Stream>(), It.IsAny<byte[]>(), It.IsAny<byte[]>(), out size, 24, 4, 300, It.IsAny<bool>()))
            .Returns(new MemoryStream());
        _ = mocks.MockRepo
            .Setup(m => m.FindAsync(It.IsAny<string>()))
            .ReturnsAsync([]);
        return new(
            mocks.MockSnapper.Object,
            mocks.MockRepo.Object);
    }

    private sealed record BagOfMocks(
        Mock<ISnapService> MockSnapper,
        Mock<IMediaRepo> MockRepo);
}
