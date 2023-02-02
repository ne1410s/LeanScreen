// <copyright file="CommonUtilsTests.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace AvCtl.Tests;

using System;
using Crypt.Encoding;
using Crypt.Keying;

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
        const string expected = "EE+QlNwpWomd7OqvqV74w1NFwp4=";

        // Act
        var hashes = CommonUtils.GetHashes("Samples", "flv$");
        var result = new DefaultKeyDeriver().DeriveKey(string.Empty, hashes).Encode(Codec.ByteBase64);

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void GetHashes_WithGoodKeySourceButNoRegex_ReturnsExpected()
    {
        // Arrange
        const string expected = "v0l5re++sfvRkyRlwBBD4aS8if0=";

        // Act
        var hashes = CommonUtils.GetHashes(Path.Combine("Samples", "Info"), null);
        var result = new DefaultKeyDeriver().DeriveKey(string.Empty, hashes).Encode(Codec.ByteBase64);

        // Assert
        result.Should().Be(expected);
    }
}
