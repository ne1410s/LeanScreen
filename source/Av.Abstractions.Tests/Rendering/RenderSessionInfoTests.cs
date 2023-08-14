// <copyright file="RenderSessionInfoTests.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace Av.Abstractions.Tests.Rendering;

using Av.Abstractions.Rendering;
using Av.Abstractions.Shared;

/// <summary>
/// Tests for the <see cref="RenderSessionInfo"/> class.
/// </summary>
public class RenderSessionInfoTests
{
    [Fact]
    public void Ctor_WithParams_PopulatesProperties()
    {
        // Arrange
        var frameSize = new Size2D(5, 10);

        // Act
        var sut = new RenderSessionInfo(frameSize);

        // Assert
        sut.FrameSize.Should().Be(frameSize);
    }
}
