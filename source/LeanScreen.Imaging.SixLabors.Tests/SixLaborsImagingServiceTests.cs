// <copyright file="SixLaborsImagingServiceTests.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace LeanScreen.Imaging.SixLabors.Tests;

using CryptoStream.Hashing;
using LeanScreen.Common;
using LeanScreen.Rendering;

/// <summary>
/// Tests for the <see cref="SixLaborsImagingService"/>.
/// </summary>
public class SixLaborsImagingServiceTests
{
    [Fact]
    public void Encode_WithValidData_ResultAsExpected()
    {
        // Arrange
        var sut = new SixLaborsImagingService();
        var rgb24 = new byte[]
        {
            117, 114, 113, 125, 120, 119, 140, 142, 141, 173, 160, 165, 144, 128,
            131, 112, 113, 121, 189, 180, 181, 132, 126, 125, 121, 123, 126,
        };

        // Act
        var result = sut.Encode(rgb24, new Size2D { Width = 3, Height = 3 });
        var encoded = result.ToArray();

        // Assert
        encoded.Length.ShouldBe(658);
        encoded.Take(10).ToArray().ShouldBeEquivalentTo(new byte[] { 255, 216, 255, 224, 0, 16, 74, 70, 73, 70 });
    }

    [Fact]
    public async Task ResizeImage_WithData_ResultAsExpected()
    {
        // Arrange
        var sut = new SixLaborsImagingService();
        var str = new MemoryStream(
        [
            66, 77, 58, 0, 0, 0, 0, 0, 0, 0, 54, 0, 0, 0, 40, 0, 0, 0, 1,
            0, 0, 0, 1, 0, 0, 0, 1, 0, 24, 0, 0, 0, 0, 0, 4, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 36, 28, 237, 0,
        ]);

        // Act
        var result = await sut.ResizeImage(str, new Size2D { Width = 2 });
        var bytes = result.ToArray();

        // Assert
        bytes.Take(5).ToArray().ShouldBeEquivalentTo(new byte[] { 66, 77, 70, 0, 0 });
    }

    [Fact]
    public void Collate_WithEmptyFrames_ThrowsException()
    {
        // Arrange
        var sut = new SixLaborsImagingService();

        // Act
        var act = () => sut.Collate([]);

        // Assert
        act.ShouldThrow<ArgumentException>().ShouldSatisfyAllConditions(
            ex => ex.Message.ShouldMatch("No frames found.*"),
            ex => ex.ParamName.ShouldBe("frames"));
    }

    [Fact]
    public void Collate_WithNullFrame_ThrowsException()
    {
        // Arrange
        var sut = new SixLaborsImagingService();

        // Act
        var act = () => sut.Collate(null!);

        // Assert
        act.ShouldThrow<ArgumentException>().ShouldSatisfyAllConditions(
            ex => ex.Message.ShouldMatch("No frames found.*"),
            ex => ex.ParamName.ShouldBe("frames"));
    }

    [Fact]
    public void Collate_WithMultipleRows_ProducesExpected()
    {
        // Arrange
        var rgb24Bytes = new byte[]
        {
            255, 0, 0, 255, 0, 0, 255, 0, 0,
            232, 162, 0, 232, 162, 0, 232, 162, 0,
            76, 177, 34, 76, 177, 34, 76, 177, 34,
            0, 255, 0, 0, 255, 0, 0, 255, 0,
            0, 0, 255, 0, 0, 255, 0, 0, 255,
            36, 28, 237, 36, 28, 237, 36, 28, 237,
        };
        var frame = new RenderedFrame { Rgb24Bytes = rgb24Bytes, Dimensions = new(18, 1) };
        var bytes3x = rgb24Bytes.Concat(rgb24Bytes).Concat(rgb24Bytes).ToArray();
        var bigFrame = new RenderedFrame { Rgb24Bytes = bytes3x, Dimensions = new(9, 6) };
        var sut = new SixLaborsImagingService();
        var expectedSha256 = new byte[]
        {
            103, 50, 76, 157, 109, 42, 198, 49, 59, 95, 158, 7, 42, 179, 136,
            82, 6, 32, 175, 162, 122, 51, 98, 179, 96, 81, 42, 108, 70, 188,
            226, 114,
        };

        // Act
        var ms = sut.Collate(Enumerable.Repeat(frame, 9).Append(bigFrame));
        var sha256 = ms.ToArray().Hash(HashType.Sha256);

        // Assert
        sha256.ShouldBeEquivalentTo(expectedSha256);
    }

    [Fact]
    public void Collate_WithCollateSize_ProducesExpected()
    {
        // Arrange
        var rgb24Bytes = new byte[]
        {
            255, 0, 0, 255, 0, 0, 255, 0, 0,
            232, 162, 0, 232, 162, 0, 232, 162, 0,
            76, 177, 34, 76, 177, 34, 76, 177, 34,
            0, 255, 0, 0, 255, 0, 0, 255, 0,
            0, 0, 255, 0, 0, 255, 0, 0, 255,
            36, 28, 237, 36, 28, 237, 36, 28, 237,
        };
        var frame = new RenderedFrame { Rgb24Bytes = rgb24Bytes, Dimensions = new(18, 1) };
        var sut = new SixLaborsImagingService();
        var expectedSha256 = new byte[]
        {
            224, 84, 19, 2, 0, 227, 248, 152, 186, 127, 3, 17, 164, 255, 170,
            136, 118, 173, 103, 251, 188, 16, 119, 196, 83, 192, 82, 145, 31,
            191, 189, 153,
        };

        // Act
        var ms = sut.Collate([frame], new() { ItemSize = new(12, 0) });
        var sha256 = ms.ToArray().Hash(HashType.Sha256);

        // Assert
        sha256.ShouldBeEquivalentTo(expectedSha256);
    }

    [Fact]
    public void Collate_WithoutBorder_ProducesExpected()
    {
        // Arrange
        var rgb24Bytes = new byte[]
        {
            255, 0, 0, 255, 0, 0, 255, 0, 0,
            232, 162, 0, 232, 162, 0, 232, 162, 0,
            76, 177, 34, 76, 177, 34, 76, 177, 34,
            0, 255, 0, 0, 255, 0, 0, 255, 0,
            0, 0, 255, 0, 0, 255, 0, 0, 255,
            36, 28, 237, 36, 28, 237, 36, 28, 237,
        };
        var frame = new RenderedFrame { Rgb24Bytes = rgb24Bytes, Dimensions = new(18, 1) };
        var sut = new SixLaborsImagingService();
        var expectedSha256 = new byte[]
        {
            224, 173, 80, 106, 222, 52, 253, 245, 24, 227, 7, 55, 182, 131, 106,
            205, 234, 244, 142, 187, 159, 197, 124, 111, 64, 99, 181, 3, 25,
            195, 21, 247,
        };

        // Act
        var ms = sut.Collate([frame], new() { UseItemBorder = false });
        var sha256 = ms.ToArray().Hash(HashType.Sha256);

        // Assert
        sha256.ShouldBeEquivalentTo(expectedSha256);
    }
}