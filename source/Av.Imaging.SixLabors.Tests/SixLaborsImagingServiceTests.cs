// <copyright file="SixLaborsImagingServiceTests.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

using Av.Abstractions.Shared;

namespace Av.Imaging.SixLabors.Tests;

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
        var result = sut.Encode(rgb24, new Dimensions2D { Width = 3, Height = 3 });
        var encoded = result.ToArray();

        // Assert
        encoded.Length.Should().Be(658);
        encoded.Take(10).Should().BeEquivalentTo(new byte[] { 255, 216, 255, 224, 0, 16, 74, 70, 73, 70 });
    }
}