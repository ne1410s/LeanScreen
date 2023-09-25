// <copyright file="CapperTests.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace Av.Tests.Services;

using Av.Abstractions.Imaging;
using Av.Abstractions.Rendering;
using Av.Abstractions.Shared;
using Av.Services;
using Moq;

/// <summary>
/// Tests for the <see cref="Capper"/> class.
/// </summary>
public class CapperTests
{
    [Fact]
    public void Collate_MissingOptions_GetPopulated()
    {
        // Arrange
        using var sut = GetSut(out var mocks);
        var opts = (CollationOptions)null!;

        // Act
        sut.Collate(opts, TimeSpan.Zero);

        // Assert
        mocks.MockImager.Verify(
            m => m.Collate(It.IsAny<IEnumerable<RenderedFrame>>(), It.Is<CollationOptions>(o => o != null)));
    }

    [Fact]
    public void Collate_MissingSize_GetsPopulated()
    {
        // Arrange
        using var sut = GetSut(out var mocks);
        var opts = new CollationOptions { ItemSize = null };

        // Act
        sut.Collate(opts, TimeSpan.Zero);

        // Assert
        mocks.MockImager.Verify(
            m => m.Collate(It.IsAny<IEnumerable<RenderedFrame>>(), It.Is<CollationOptions>(o => o.ItemSize != null)));
    }

    [Fact]
    public void SetSource_WhenCalled_SetsRendererSource()
    {
        // Arrange
        using var sut = GetSut(out var mocks);
        const string filePath = "file.txt";
        var key = new byte[] { 1, 2, 3 };
        var size = new Size2D { Width = 10, Height = 10 };

        // Act
        sut.SetSource(filePath, key, size);

        // Assert
        mocks.MockRenderer.Verify(m => m.SetSource(filePath, key, size));
    }

    ////[Fact]
    ////public void Generate_NPositions_CallsRendererNTimes()
    ////{
    ////    // Arrange
    ////    var mockRenderer = new Mock<IRenderingService>();
    ////    var sut = new ThumbnailGenerator(mockRenderer.Object);
    ////    var times = new TimeSpan[] { TimeSpan.Zero, TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(10) };
    ////    var calls = 0;

    ////    // Act
    ////    sut.Generate((_, _) => calls++, times);

    ////    // Assert
    ////    calls.Should().Be(times.Length);
    ////    mockRenderer.Verify(m => m.RenderAt(It.IsAny<TimeSpan>()), Times.Exactly(times.Length));
    ////}

    ////[Fact]
    ////public void Generate_NCount_CallsRendererNTimesEvenly()
    ////{
    ////    // Arrange
    ////    var mockRenderer = new Mock<IRenderingService>();
    ////    var mockMedia = new MediaInfo(TimeSpan.FromSeconds(2), default, 0, 0);
    ////    mockRenderer.Setup(m => m.Media).Returns(mockMedia);
    ////    var sut = new ThumbnailGenerator(mockRenderer.Object);
    ////    const int times = 3;
    ////    var calls = 0;

    ////    // Act
    ////    sut.Generate((_, _) => calls++, times);

    ////    // Assert
    ////    calls.Should().Be(times);
    ////    mockRenderer.Verify(m => m.RenderAt(TimeSpan.FromSeconds(0)), Times.Once());
    ////    mockRenderer.Verify(m => m.RenderAt(TimeSpan.FromSeconds(1)), Times.Once());
    ////    mockRenderer.Verify(m => m.RenderAt(TimeSpan.FromSeconds(2)), Times.Once());
    ////}

    ////[Fact]
    ////public void Generate_NPositionsIncludingNonDefault_HonoursSpecified()
    ////{
    ////    // Arrange
    ////    var actualCallTimes = new List<TimeSpan>();
    ////    var mockRenderer = new Mock<IRenderingService>();
    ////    var mockMedia = new MediaInfo(TimeSpan.FromSeconds(2), default, 0, 0);
    ////    mockRenderer.Setup(m => m.Media).Returns(mockMedia);
    ////    mockRenderer.Setup(m => m.RenderAt(It.IsAny<TimeSpan>())).Callback((TimeSpan ts) => actualCallTimes.Add(ts));
    ////    var sut = new ThumbnailGenerator(mockRenderer.Object);
    ////    var times = new TimeSpan[] { default, default, TimeSpan.FromSeconds(1.3) };

    ////    // Act
    ////    sut.Generate((_, _) => { }, times);

    ////    // Assert
    ////    actualCallTimes.Should().BeEquivalentTo(times);
    ////}

    ////[Fact]
    ////public void Generate_SinglePositionOfDefault_UsesPositionAtZero()
    ////{
    ////    // Arrange
    ////    var mockRenderer = new Mock<IRenderingService>();
    ////    var mockMedia = new MediaInfo(TimeSpan.FromSeconds(2), default, 0, 0);
    ////    mockRenderer.Setup(m => m.Media).Returns(mockMedia);
    ////    var sut = new ThumbnailGenerator(mockRenderer.Object);
    ////    var times = new TimeSpan[1];

    ////    // Act
    ////    sut.Generate((_, _) => { }, times);

    ////    // Assert
    ////    mockRenderer.Verify(m => m.RenderAt(TimeSpan.Zero), Times.Once());
    ////}

    ////[Fact]
    ////public void Generate_NullCallback_ThrowsException()
    ////{
    ////    // Arrange
    ////    var mockRenderer = new Mock<IRenderingService>();
    ////    var sut = new ThumbnailGenerator(mockRenderer.Object);
    ////    var times = new TimeSpan[1];

    ////    // Act
    ////    var act = () => sut.Generate(null!, times);

    ////    // Assert
    ////    act.Should().Throw<ArgumentNullException>().WithParameterName("onRendered");
    ////}

    ////[Fact]
    ////public void Generate_NullTimings_ThrowsException()
    ////{
    ////    // Arrange
    ////    var mockRenderer = new Mock<IRenderingService>();
    ////    var sut = new ThumbnailGenerator(mockRenderer.Object);

    ////    // Act
    ////    var act = () => sut.Generate((_, _) => { }, null!);

    ////    // Assert
    ////    act.Should().Throw<ArgumentNullException>().WithParameterName("times");
    ////}

    private static Capper GetSut(out BagOfMocks mocks)
    {
        mocks = new()
        {
            MockRenderer = new Mock<IRenderingService>(),
            MockImager = new Mock<IImagingService>(),
        };

        return new Capper(mocks.MockRenderer.Object, mocks.MockImager.Object);
    }

    private class BagOfMocks
    {
        public Mock<IRenderingService> MockRenderer { get; init; } = default!;

        public Mock<IImagingService> MockImager { get; init; } = default!;
    }
}
