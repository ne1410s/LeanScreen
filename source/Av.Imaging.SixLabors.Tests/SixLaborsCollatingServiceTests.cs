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
        var bytes4x = rgb24Bytes.Concat(rgb24Bytes).Concat(rgb24Bytes).Concat(rgb24Bytes).ToArray();
        var bigFrame = new RenderedFrame { Rgb24Bytes = bytes4x, Dimensions = new(36, 2) };
        var sut = new SixLaborsCollatingService();
        var expectedSha256 = new byte[]
        {
            227, 176, 196, 66, 152, 252, 28, 20, 154, 251, 244, 200, 153, 111, 185, 36,
            39, 174, 65, 228, 100, 155, 147, 76, 164, 149, 153, 27, 120, 82, 184, 85,
        };

        // Act
        var ms = sut.Collate(Enumerable.Repeat(frame, 9).Append(bigFrame));
        var sha256 = SHA256.Create().ComputeHash(ms);

        // Assert
        sha256.Should().BeEquivalentTo(expectedSha256);
    }
}
