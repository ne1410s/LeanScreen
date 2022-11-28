// <copyright file="SixLaborsCollatingServiceTests.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace Av.Imaging.SixLabors.Tests;

using System.Security.Cryptography;
using Av.Abstractions.Rendering;

/// <summary>
/// Tests for the <see cref="SixLaborsCollatingService"/>.
/// </summary>
public class SixLaborsCollatingServiceTests
{
    [Fact]
    public void RenderCollation_WithMultipleRows_ProducesExpected()
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
        var sut = new SixLaborsCollatingService();
        var expectedSha256 = new byte[]
        {
            103, 50, 76, 157, 109, 42, 198, 49, 59, 95, 158, 7, 42, 179, 136,
            82, 6, 32, 175, 162, 122, 51, 98, 179, 96, 81, 42, 108, 70, 188,
            226, 114,
        };

        // Act
        var ms = sut.Collate(Enumerable.Repeat(frame, 9).Append(bigFrame));
        var sha256 = SHA256.Create().ComputeHash(ms);

        // Assert
        sha256.Should().BeEquivalentTo(expectedSha256);
    }

    [Fact]
    public void RenderCollation_WithCollateSize_ProducesExpected()
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
        var sut = new SixLaborsCollatingService();
        var expectedSha256 = new byte[]
        {
            224, 84, 19, 2, 0, 227, 248, 152, 186, 127, 3, 17, 164, 255, 170,
            136, 118, 173, 103, 251, 188, 16, 119, 196, 83, 192, 82, 145, 31,
            191, 189, 153,
        };

        // Act
        var ms = sut.Collate(new[] { frame }, new() { ItemSize = new(12, 0) });
        var sha256 = SHA256.Create().ComputeHash(ms);

        // Assert
        sha256.Should().BeEquivalentTo(expectedSha256);
    }
}
