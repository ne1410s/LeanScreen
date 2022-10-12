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
    public void Generate_NTimes_CallsRendererNTimes()
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
        mockRenderer.Setup(m => m.Duration).Returns(TimeSpan.FromSeconds(2));
        var sut = new ThumbnailGenerator(mockRenderer.Object);
        var times = 3;
        var calls = 0;

        // Act
        sut.Generate((_, _) => calls++, times);

        // Assert
        calls.Should().Be(times);
        mockRenderer.Verify(m => m.RenderAt(TimeSpan.FromSeconds(0)), Times.Once());
        mockRenderer.Verify(m => m.RenderAt(TimeSpan.FromSeconds(1)), Times.Once());
        mockRenderer.Verify(m => m.RenderAt(TimeSpan.FromSeconds(2)), Times.Once());
    }
}
