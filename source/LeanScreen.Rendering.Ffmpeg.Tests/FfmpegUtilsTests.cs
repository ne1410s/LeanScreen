﻿// <copyright file="FfmpegUtilsTests.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace LeanScreen.Rendering.Ffmpeg.Tests;

using System.Globalization;
using FFmpeg.AutoGen;
using LeanScreen.Rendering.Ffmpeg.Decoding;

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
        actual.ShouldBe(expected);
    }

    [Fact]
    public void ThrowExceptionIfError_IsError_ThrowsException()
    {
        // Arrange
        const int code = -1;

        // Act
        Action act = () => code.avThrowIfError();

        // Assert
        _ = act.ShouldThrow<Exception>();
    }

    [Fact]
    public void ThrowExceptionIfError_IsNotError_DoesNotThrow()
    {
        // Arrange
        const int code = 0;

        // Act
        var act = () => code.avThrowIfError();

        // Assert
        _ = act.ShouldNotThrow();
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
        result.ShouldBe(25);
    }

    [Fact]
    public void ToTimeSpan_WithValues_ReturnsExpected()
    {
        // Arrange
        const double value = 10;

        // Act
        var result = value.ToTimeSpan(new AVRational { num = 2, den = 5 }).TotalSeconds;

        // Assert
        result.ShouldBe(4);
    }

    [Fact]
    public void ToTimeSpan_WithNoOptsValueAndRational_ReturnsZero()
    {
        // Arrange
        var noOptsValue = ffmpeg.AV_NOPTS_VALUE;

        // Act
        var result = ((double)noOptsValue).ToTimeSpan(new AVRational { num = 1, den = 12 });

        // Assert
        result.ShouldBe(TimeSpan.Zero);
    }

    [Fact]
    public void ToTimeSpan_WithZeroDenominatorRational_DoesNotError()
    {
        // Arrange
        const double pts = 1000000;

        // Act
        var result = pts.ToTimeSpan(new AVRational { num = 1, den = 0 });

        // Assert
        result.ShouldBe(TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void ToTimeSpan_WithNoOptsValueAndDouble_ReturnsZero()
    {
        // Arrange
        var noOptsValue = (double)ffmpeg.AV_NOPTS_VALUE;

        // Act
        var result = noOptsValue.ToTimeSpan(new AVRational { num = 1, den = 25 });

        // Assert
        result.ShouldBe(TimeSpan.Zero);
    }

    [Fact]
    public void SetupLogging_Disposed_ExceptionSwallowed()
    {
        // Arrange
        var hitLogger = false;
        FfmpegUtils.LogLevel = ffmpeg.AV_LOG_VERBOSE;
        FfmpegUtils.Logger = (_, _) =>
        {
            hitLogger = true;
            throw new ArithmeticException();
        };

        // Act
        var act = () => new PhysicalFfmpegDecoding("Samples/sample.mkv");

        // Assert
        _ = act.ShouldNotThrow();
        hitLogger.ShouldBeTrue();

        // Reset
        FfmpegUtils.Logger = null;
        FfmpegUtils.LogLevel = ffmpeg.AV_LOG_WARNING;
    }
}
