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
    public void Collate_BespokeSize_GetsHonoured()
    {
        // Arrange
        using var sut = GetSut(out var mocks);
        var requestSize = new Size2D(12, 17);
        var opts = new CollationOptions { ItemSize = requestSize };

        // Act
        sut.Collate(opts, TimeSpan.Zero);

        // Assert
        mocks.MockImager.Verify(
            m => m.Collate(
                It.IsAny<IEnumerable<RenderedFrame>>(),
                It.Is<CollationOptions>(o => requestSize.Equals(o.ItemSize))));
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

    [Fact]
    public void Snap_WithRelativePosition_YieldsExpectedTime()
    {
        // Arrange
        using var sut = GetSut(out var mocks);
        const double relative = 0.75;
        var duration = TimeSpan.FromSeconds(4);
        var expectedRenderAt = duration * relative;
        var size = new Size2D { Height = 10, Width = 10 };
        mocks.MockRenderer.Setup(m => m.Media).Returns(new MediaInfo(duration, size, 40, 10));

        // Act
        sut.Snap(relative);

        // Assert
        mocks.MockRenderer.Verify(m => m.RenderAt(expectedRenderAt));
    }

    [Fact]
    public void Snap_WhenCalled_SequenceIsUnity()
    {
        // Arrange
        using var sut = GetSut(out _);

        // Act
        var result = sut.Snap(TimeSpan.Zero);

        // Assert
        result.SequenceNumber.Should().Be(1);
    }

    private static Capper GetSut(out BagOfMocks mocks)
    {
        mocks = new()
        {
            MockRenderer = new Mock<IRenderingService>(),
            MockImager = new Mock<IImagingService>(),
        };

        mocks.MockRenderer
            .Setup(m => m.RenderAt(It.IsAny<TimeSpan>()))
            .Returns(new RenderedFrame());

        return new Capper(mocks.MockRenderer.Object, mocks.MockImager.Object);
    }

    private class BagOfMocks
    {
        public Mock<IRenderingService> MockRenderer { get; init; } = default!;

        public Mock<IImagingService> MockImager { get; init; } = default!;
    }
}
