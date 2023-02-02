// <copyright file="CommonUtilsTests.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace AvCtl.Tests;

using System;

/// <summary>
/// Tests for the <see cref="CommonUtils"/> class.
/// </summary>
public class CommonUtilsTests
{
    [Fact]
    public void Blend_EmptyInput_ReturnsEmpty()
    {
        // Arrange
        var input = Array.Empty<string>();

        // Act
        var result = input.Blend();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void Blend_SingleInput_ReturnsSingle()
    {
        // Arrange
        var input = new[] { "hello" };

        // Act
        var result = input.Blend();

        // Assert
        result.Should().Be(input[0]);
    }

    [Fact]
    public void Blend_LongRemainder_ReturnsExpected()
    {
        // Arrange
        var input = new[] { "hi", "12345678" };
        const string expected = "h1i2h3i4h5i6h7i8";

        // Act
        var result = input.Blend();

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void GetHashes_WithBadKeySource_ThrowsException()
    {
        // Arrange
        var nonExistingDir = Guid.NewGuid().ToString();

        // Act
        var act = () => CommonUtils.GetHashes(nonExistingDir, null);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("sourceDir")
            .WithMessage($"Directory not found: {nonExistingDir}*");
    }

    [Fact]
    public void GetHashes_WithoutAnyKeySource_ReturnsEmpty()
    {
        // Arrange & Act
        var actual = CommonUtils.GetHashes(null, null);

        // Assert
        actual.Should().BeEmpty();
    }

    [Fact]
    public void GetHashes_WithGoodKeySourceButFilteringRegex_ReturnsEmpty()
    {
        // Arrange
        const string keySource = "Samples";
        const string keyRegex = "xxyyzzxyz";

        // Act
        var actual = CommonUtils.GetHashes(keySource, keyRegex);

        // Assert
        actual.Should().BeEmpty();
    }

    [Fact]
    public void GetHashes_WithGoodKeySourceAndRegex_ReturnsExpected()
    {
        // Arrange
        var expected = new byte[][]
        {
            new byte[] { 184, 130, 199, 245, 65, 134, 232, 154, 193, 193, 249, 8, 187, 88, 94, 0, 34, 40, 218, 115 },
        };

        // Act
        var actual = CommonUtils.GetHashes("Samples", "flv$");

        // Assert
        actual.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void GetHashes_WithGoodKeySourceButNoRegex_ReturnsExpected()
    {
        // Arrange
        var expected = new byte[][]
        {
            new byte[] { 156, 170, 68, 217, 234, 120, 57, 174, 103, 189, 137, 149, 240, 6, 182, 212, 71, 4, 17, 36 },
            new byte[] { 1, 129, 215, 136, 10, 207, 12, 68, 58, 87, 205, 37, 16, 165, 184, 20, 71, 250, 87, 126 },
            new byte[] { 232, 159, 75, 147, 66, 202, 184, 126, 124, 2, 173, 184, 30, 13, 42, 120, 9, 110, 6, 145 },
            new byte[] { 53, 192, 14, 54, 171, 65, 119, 248, 64, 245, 67, 208, 12, 1, 165, 134, 25, 32, 86, 112 },
            new byte[] { 148, 206, 67, 137, 78, 64, 77, 42, 37, 237, 157, 118, 169, 10, 147, 8, 30, 218, 104, 247 },
            new byte[] { 59, 230, 202, 220, 187, 190, 250, 5, 80, 41, 153, 85, 103, 205, 16, 26, 54, 204, 224, 126 },
        };

        // Act
        var actual = CommonUtils.GetHashes(Path.Combine("Samples", "Info"), null);

        // Assert
        actual.Should().BeEquivalentTo(expected);
    }
}
