// <copyright file="ThumbnailGeneratorTests.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

using Av.Abstractions.Rendering;
using Av.Services;

namespace Av.Tests.Services;

/// <summary>
/// Tests for the <see cref="ThumbnailGenerator"/>.
/// </summary>
public class ThumbnailGeneratorTests
{
    [Fact]
    public void Generate_NPositions_CallsRendererNTimes()
    {
        // Arrange
        var mockRenderer = new Mock<IRenderingService>();
        var sut = new ThumbnailGenerator(mockRenderer.Object);
        var times = new TimeSpan[] { TimeSpan.Zero, TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(10) };
        var calls = 0;

        // Act
        sut.Generate((_, _) => calls++, times);

        // Assert
        calls.Should().Be(times.Length);
        mockRenderer.Verify(m => m.RenderAt(It.IsAny<TimeSpan>()), Times.Exactly(times.Length));
    }

    [Fact]
    public void Generate_NCount_CallsRendererNTimesEvenly()
    {
        // Arrange
        var mockRenderer = new Mock<IRenderingService>();
        var mockMedia = new MediaInfo(TimeSpan.FromSeconds(2), default, 0, 0);
        mockRenderer.Setup(m => m.Media).Returns(mockMedia);
        var sut = new ThumbnailGenerator(mockRenderer.Object);
        const int times = 3;
        var calls = 0;

        // Act
        sut.Generate((_, _) => calls++, times);

        // Assert
        calls.Should().Be(times);
        mockRenderer.Verify(m => m.RenderAt(TimeSpan.FromSeconds(0)), Times.Once());
        mockRenderer.Verify(m => m.RenderAt(TimeSpan.FromSeconds(1)), Times.Once());
        mockRenderer.Verify(m => m.RenderAt(TimeSpan.FromSeconds(2)), Times.Once());
    }

    [Fact]
    public void Generate_NPositionsIncludingNonDefault_HonoursSpecified()
    {
        // Arrange
        var actualCallTimes = new List<TimeSpan>();
        var mockRenderer = new Mock<IRenderingService>();
        var mockMedia = new MediaInfo(TimeSpan.FromSeconds(2), default, 0, 0);
        mockRenderer.Setup(m => m.Media).Returns(mockMedia);
        mockRenderer.Setup(m => m.RenderAt(It.IsAny<TimeSpan>())).Callback((TimeSpan ts) => actualCallTimes.Add(ts));
        var sut = new ThumbnailGenerator(mockRenderer.Object);
        var times = new TimeSpan[] { default, default, TimeSpan.FromSeconds(1.3) };

        // Act
        sut.Generate((_, _) => { }, times);

        // Assert
        actualCallTimes.Should().BeEquivalentTo(times);
    }

    [Fact]
    public void Generate_SinglePositionOfDefault_UsesPositionAtZero()
    {
        // Arrange
        var mockRenderer = new Mock<IRenderingService>();
        var mockMedia = new MediaInfo(TimeSpan.FromSeconds(2), default, 0, 0);
        mockRenderer.Setup(m => m.Media).Returns(mockMedia);
        var sut = new ThumbnailGenerator(mockRenderer.Object);
        var times = new TimeSpan[1];

        // Act
        sut.Generate((_, _) => { }, times);

        // Assert
        mockRenderer.Verify(m => m.RenderAt(TimeSpan.Zero), Times.Once());
    }

    [Fact]
    public void Generate_NullCallback_ThrowsException()
    {
        // Arrange
        var mockRenderer = new Mock<IRenderingService>();
        var sut = new ThumbnailGenerator(mockRenderer.Object);
        var times = new TimeSpan[1];

        // Act
        var act = () => sut.Generate(null!, times);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("onRendered");
    }

    [Fact]
    public void Generate_NullTimings_ThrowsException()
    {
        // Arrange
        var mockRenderer = new Mock<IRenderingService>();
        var sut = new ThumbnailGenerator(mockRenderer.Object);

        // Act
        var act = () => sut.Generate((_, _) => { }, null!);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("times");
    }
}
