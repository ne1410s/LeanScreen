﻿using FFmpeg.AutoGen;
using Newtonsoft.Json.Bson;

namespace Av.Rendering.Ffmpeg.Tests
{
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
            var position = TimeSpan.Parse(positionString);
            var expected = TimeSpan.Parse(expectedString);

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
            var act = () => code.ThrowExceptionIfError();

            // Assert
            act.Should().ThrowExactly<ApplicationException>()
                .WithMessage("No such process");
        }

        [Fact]
        public void ThrowExceptionIfError_IsNotError_DoesNotThrow()
        {
            // Arrange
            const int code = 0;

            // Act
            var act = () => code.ThrowExceptionIfError();

            // Assert
            act.Should().NotThrow();
        }

        [Fact]
        public void ToTimeSpan_WithNoOptsValueAndRational_ReturnsMin()
        {
            // Arrange
            var noOptsValue = ffmpeg.AV_NOPTS_VALUE;

            // Act
            var result = noOptsValue.ToTimeSpan(new AVRational { num = 1, den = 12 });

            // Assert
            result.Should().Be(TimeSpan.MinValue);
        }

        [Fact]
        public void ToTimeSpan_WithZeroDenominatorRational_DoesNotError()
        {
            // Arrange
            const long pts = 1000000;

            // Act
            var result = pts.ToTimeSpan(new AVRational { num = 1, den = 0 });

            // Assert
            result.Should().Be(TimeSpan.FromSeconds(1));
        }

        [Fact]
        public void ToTimeSpan_WithNoOptsValueAndDouble_ReturnsMin()
        {
            // Arrange
            var noOptsValue = ffmpeg.AV_NOPTS_VALUE;

            // Act
            var result = noOptsValue.ToTimeSpan(0.0124);

            // Assert
            result.Should().Be(TimeSpan.MinValue);
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
        }
    }
}
