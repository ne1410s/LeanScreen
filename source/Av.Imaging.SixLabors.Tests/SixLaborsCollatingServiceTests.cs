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
            125, 243, 15, 61, 154, 39, 107, 33, 152, 169, 68, 134, 240, 83, 104,
            215, 245, 134, 170, 92, 79, 8, 82, 88, 116, 106, 8, 209, 66, 226, 225, 240,
        };

        // Act
        var ms = sut.Collate(Enumerable.Repeat(frame, 9).Append(bigFrame));
        var sha256 = SHA256.Create().ComputeHash(ms);

        // Assert
        sha256.Should().BeEquivalentTo(expectedSha256);
    }
}
