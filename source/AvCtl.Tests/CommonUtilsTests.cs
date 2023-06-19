﻿// <copyright file="CommonUtilsTests.cs" company="ne1410s">
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
        // Act
        var actual = CommonUtils.GetHashes(Path.Combine("Samples", "Info"), null);

        // Assert
        actual.Length.Should().Be(5);
        Array.TrueForAll(actual, a => a.Length == 20).Should().BeTrue();
    }
}
