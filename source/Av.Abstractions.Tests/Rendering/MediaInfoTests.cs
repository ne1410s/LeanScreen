// <copyright file="MediaInfoTests.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

using Av.Abstractions.Rendering;
using Av.Abstractions.Shared;

namespace Av.Abstractions.Tests.Rendering;

/// <summary>
/// Tests for the <see cref="MediaInfo"/> class.
/// </summary>
public class MediaInfoTests
{
    [Fact]
    public void Ctor_WithParams_PopulatesProperties()
    {
        // Arrange
        var duration = TimeSpan.FromSeconds(1);
        var dimensions = new Size2D(5, 10);
        const int totalFrames = 10;
        const double frameRate = 2.2;

        // Act
        var sut = new MediaInfo(duration, dimensions, totalFrames, frameRate);

        // Assert
        sut.Duration.Should().Be(duration);
        sut.Dimensions.Should().Be(dimensions);
        sut.TotalFrames.Should().Be(totalFrames);
        sut.FrameRate.Should().Be(frameRate);
    }
}
