// <copyright file="SixLaborsCollatingServiceTests.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

using System.Security.Cryptography;
using Av.Abstractions.Rendering;

namespace Av.Imaging.SixLabors.Tests;

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
            156, 100, 149, 201, 102, 108, 24, 6, 125, 156, 198,
            34, 109, 24, 57, 239, 88, 6, 9, 211, 143, 160, 74,
            252, 226, 158, 53, 215, 218, 80, 216, 212,
        };

        // Act
        var ms = sut.Collate(Enumerable.Repeat(frame, 9).Append(bigFrame));
        var sha256 = SHA256.Create().ComputeHash(ms);

        // Assert
        sha256.Should().BeEquivalentTo(expectedSha256);
    }
}
