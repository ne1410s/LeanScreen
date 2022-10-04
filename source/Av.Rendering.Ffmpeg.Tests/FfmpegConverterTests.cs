using Av.Abstractions.Shared;

namespace Av.Rendering.Ffmpeg.Tests;

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
        var sourceSize = new Dimensions2D { Width = width, Height = height };
        var validDestSize = new Dimensions2D { Width = 1, Height = 1 };

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
        var validSourceSize = new Dimensions2D { Width = 1, Height = 1 };
        var destSize = new Dimensions2D { Width = width, Height = height };

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
        FfmpegUtils.SetupBinaries();
        var size = new Dimensions2D { Width = 1, Height = 1 };
        var sut = new FfmpegConverter(size, default, size);

        // Act
        var act = () => sut.Dispose();

        // Assert
        act.Should().NotThrow();
    }
}
