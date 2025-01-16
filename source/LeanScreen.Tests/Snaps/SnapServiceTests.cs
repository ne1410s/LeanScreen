// <copyright file="SnapServiceTests.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace LeanScreen.Tests.Snaps;

using LeanScreen.Imaging;
using LeanScreen.Rendering;
using LeanScreen.Snaps;

/// <summary>
/// Tests for the <see cref="SnapService"/> class.
/// </summary>
public class SnapServiceTests
{
    [Fact]
    public void Collate_IsCompact_SendsExpectedConfig()
    {
        // Arrange
        var sut = GetSut(out var mockImager);
        var expectedOpts = new CollationOptions
        {
            Top = 0,
            Sides = 0,
            Bottom = 0,
            SpaceX = 0,
            SpaceY = 0,
            ItemSize = new(),
            UseItemBorder = false,
        };

        // Act
        _ = sut.Collate(new MemoryStream(), [], [], out _, compact: true);

        // Assert
        mockImager.Verify(m => m.Collate(
            It.IsAny<IEnumerable<RenderedFrame>>(),
            It.Is<CollationOptions>(o => o == expectedOpts)));
    }

    private static SnapService GetSut(out Mock<IImagingService> mockImager)
    {
        var mockRenderer = new Mock<IRenderingSession>();
        _ = mockRenderer.Setup(m => m.Media).Returns(new MediaInfo(default, default, default, default));

        var mockRendererFactory = new Mock<IRenderingSessionFactory>();
        _ = mockRendererFactory
            .Setup(m => m.Create(It.IsAny<Stream>(), It.IsAny<byte[]>(), It.IsAny<byte[]>(), It.IsAny<int?>()))
            .Returns(mockRenderer.Object);

        mockImager = new Mock<IImagingService>();
        return new SnapService(mockRendererFactory.Object, mockImager.Object);
    }
}
