﻿// <copyright file="MediaInfoTests.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace LeanScreen.Abstractions.Tests.Rendering;

using LeanScreen.Common;
using LeanScreen.Rendering;

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
        sut.Duration.ShouldBe(duration);
        sut.Dimensions.ShouldBe(dimensions);
        sut.TotalFrames.ShouldBe(totalFrames);
        sut.FrameRate.ShouldBe(frameRate);
    }
}
