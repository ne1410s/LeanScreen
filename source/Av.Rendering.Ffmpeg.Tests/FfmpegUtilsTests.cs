// <copyright file="FfmpegUtilsTests.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace Av.Rendering.Ffmpeg.Tests;

using System.Globalization;
using FFmpeg.AutoGen;

/// <summary>
/// Tests for <see cref="FfmpegUtils"/>.
/// </summary>
public class FfmpegUtilsTests
{
    [Theory]
    [InlineData("-1:0:0", "0:0:0")]
    [InlineData("1:0:0", "0:12:30")]
    public void Clamp_VaryingValue_ReturnsExpected(string positionString, string expectedString)
    {
        // Arrange
        var duration = TimeSpan.FromMinutes(12.5);
        var position = TimeSpan.Parse(positionString, CultureInfo.InvariantCulture);
        var expected = TimeSpan.Parse(expectedString, CultureInfo.InvariantCulture);

        // Act
        var actual = position.Clamp(duration);

        // Assert
        actual.Should().Be(expected);
    }

    [Fact]
    public void ThrowExceptionIfError_IsError_ThrowsException()
    {
        // Arrange
        FfmpegUtils.SetupBinaries();
        const int code = -3;

        // Act
        var act = () => code.avThrowIfError();

        // Assert
        act.Should().ThrowExactly<InvalidOperationException>()
            .WithMessage("No such process");
    }

    [Fact]
    public void ThrowExceptionIfError_IsNotError_DoesNotThrow()
    {
        // Arrange
        const int code = 0;

        // Act
        var act = () => code.avThrowIfError();

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void ToLong_WithValues_ReturnsExpected()
    {
        // Arrange
        var value = TimeSpan.FromSeconds(10);
        var timebase = new AVRational { num = 2, den = 5 };

        // Act
        var result = value.ToLong(timebase);

        // Assert
        result.Should().Be(25);
    }

    [Fact]
    public void ToTimeSpan_WithValues_ReturnsExpected()
    {
        // Arrange
        const double value = 10;

        // Act
        var result = value.ToTimeSpan(new AVRational { num = 2, den = 5 }).TotalSeconds;

        // Assert
        result.Should().Be(4);
    }

    [Fact]
    public void ToTimeSpan_WithNoOptsValueAndRational_ReturnsZero()
    {
        // Arrange
        var noOptsValue = ffmpeg.AV_NOPTS_VALUE;

        // Act
        var result = ((double)noOptsValue).ToTimeSpan(new AVRational { num = 1, den = 12 });

        // Assert
        result.Should().Be(TimeSpan.Zero);
    }

    [Fact]
    public void ToTimeSpan_WithZeroDenominatorRational_DoesNotError()
    {
        // Arrange
        const double pts = 1000000;

        // Act
        var result = pts.ToTimeSpan(new AVRational { num = 1, den = 0 });

        // Assert
        result.Should().Be(TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void ToTimeSpan_WithNoOptsValueAndDouble_ReturnsZero()
    {
        // Arrange
        var noOptsValue = (double)ffmpeg.AV_NOPTS_VALUE;

        // Act
        var result = noOptsValue.ToTimeSpan(new AVRational { num = 1, den = 25 });

        // Assert
        result.Should().Be(TimeSpan.Zero);
    }

    [Theory]
    [InlineData(true, "ffmpeg")]
    [InlineData(false, "/lib/x86_64-linux-gnu")]
    public void SetupBinaries_VaryingOS_SetsExpectedPath(bool isWindows, string expectedPath)
    {
        // Arrange & Act
        FfmpegUtils.SetupBinaries(isWindows);

        // Assert
        ffmpeg.RootPath.Should().Be(expectedPath);

        // Reset
        FfmpegUtils.SetupBinaries();
    }
}
