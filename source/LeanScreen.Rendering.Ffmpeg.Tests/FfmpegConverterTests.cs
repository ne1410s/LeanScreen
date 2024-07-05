// <copyright file="FfmpegConverterTests.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace LeanScreen.Rendering.Ffmpeg.Tests;

using System.Reflection;
using LeanScreen.Common;

/// <summary>
/// Tests for the <see cref="FfmpegConverter"/>.
/// </summary>
public class FfmpegConverterTests
{
    [Theory]
    [InlineData(0, 1)]
    [InlineData(-1, 1)]
    [InlineData(1, 0)]
    [InlineData(1, -1)]
    public void Ctor_BadSourceSize_ThrowsArgumentException(int width, int height)
    {
        // Arrange
        var sourceSize = new Size2D { Width = width, Height = height };
        var validDestSize = new Size2D { Width = 1, Height = 1 };

        // Act
        var act = () => new FfmpegConverter(sourceSize, default, validDestSize);

        // Assert
        act.Should().ThrowExactly<ArgumentException>()
            .WithMessage("The size is invalid. (Parameter 'sourceSize')");
    }

    [Theory]
    [InlineData(0, 1)]
    [InlineData(-1, 1)]
    [InlineData(1, 0)]
    [InlineData(1, -1)]
    public void Ctor_BadDestinationSize_ThrowsArgumentException(int width, int height)
    {
        // Arrange
        var validSourceSize = new Size2D { Width = 1, Height = 1 };
        var destSize = new Size2D { Width = width, Height = height };

        // Act
        var act = () => new FfmpegConverter(validSourceSize, default, destSize);

        // Assert
        act.Should().ThrowExactly<ArgumentException>()
            .WithMessage("The size is invalid. (Parameter 'destinationSize')");
    }

    [Fact]
    public void Dispose_WhenCalled_DoesNotError()
    {
        // Arrange
        var size = new Size2D(1, 1);
        var sut = new FfmpegConverter(size, default, size);

        // Act
        sut.Dispose();

        // Assert
        1.Should().Be(1);
    }

    [Fact]
    public void Dispose_WhenCalled_CanNoLongerRender()
    {
        // Arrange
        var size = new Size2D { Width = 1, Height = 1 };
        var sut = new FfmpegConverter(size, default, size);
        var dispInfo = sut.GetType().GetField(
            "convertedFrameBufferPtr", BindingFlags.Instance | BindingFlags.NonPublic);

        // Act
        sut.Dispose();
        var result = (IntPtr)dispInfo!.GetValue(sut)!;

        // Assert
        result.Should().Be(IntPtr.Zero);
    }
}
