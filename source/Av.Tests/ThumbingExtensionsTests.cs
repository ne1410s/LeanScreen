// <copyright file="ThumbingExtensionsTests.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

using Av.Abstractions.Shared;

namespace Av.Tests;

/// <summary>
/// Tests for the <see cref="ThumbingExtensions"/>.
/// </summary>
public class ThumbingExtensionsTests
{
    [Fact]
    public void DistributeEvenly_WithValue_ReturnsExpected()
    {
        // Arrange
        var value = TimeSpan.FromSeconds(10);
        var expected = new TimeSpan[] { TimeSpan.Zero, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(10) };

        // Act
        var result = value.DistributeEvenly(3);

        // Assert
        result.Should().BeEquivalentTo(expected);
    }

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
        var act = () => source.ResizeTo(target);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Dimensions invalid. (Parameter 'source')");
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
        var act = () => source.ResizeTo(target);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Dimensions invalid. (Parameter 'target')");
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
        result.Should().Be(expected);
    }
}
