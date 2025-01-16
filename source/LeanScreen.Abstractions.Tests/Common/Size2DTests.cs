// <copyright file="Size2DTests.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace LeanScreen.Abstractions.Tests.Common;

using LeanScreen.Common;

/// <summary>
/// Tests for the <see cref="Size2D"/>.
/// </summary>
public class Size2DTests
{
    [Theory]
    [InlineData(0, 10)]
    [InlineData(-1, 10)]
    [InlineData(10, 0)]
    [InlineData(10, -1)]
    public void ResizeTo_InvalidSource_ThrowsException(int width, int height)
    {
        // Arrange
        var source = new Size2D(width, height);
        var target = new Size2D(100, 50);

        // Act
        Action act = () => source.ResizeTo(target);

        // Assert
        act.ShouldThrow<ArgumentException>().Message.ShouldBe("Source dimensions invalid.");
    }

    [Theory]
    [InlineData(0, 0)]
    [InlineData(-1, 10)]
    [InlineData(10, -1)]
    public void ResizeTo_InvalidTarget_ThrowsException(int width, int height)
    {
        // Arrange
        var source = new Size2D(100, 50);
        var target = new Size2D(width, height);

        // Act
        Action act = () => source.ResizeTo(target);

        // Assert
        act.ShouldThrow<ArgumentException>().Message.ShouldBe("Target dimensions invalid. (Parameter 'target')");
    }

    [Theory]
    [InlineData(10, 0, 10, 6)]
    [InlineData(0, 6, 11, 6)]
    [InlineData(12, 6, 12, 6)]
    public void ResizeTo_WithDimensions_ResultExpected(int pWidth, int pHeight, int expWidth, int expHeight)
    {
        // Arrange
        var source = new Size2D(100, 55);
        var target = new Size2D(pWidth, pHeight);
        var expected = new Size2D(expWidth, expHeight);

        // Act
        var result = source.ResizeTo(target);

        // Assert
        result.ShouldBe(expected);
    }
}
