// <copyright file="CryptoExtensionsTests.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace LeanScreen.Tests;

/// <summary>
/// Tests for the <see cref="CryptoExtensions"/> class.
/// </summary>
public class CryptoExtensionsTests
{
    [Fact]
    public void Encrypt_WithStream_ResetsPosition()
    {
        // Arrange
        using var stream = new MemoryStream();
        stream.Write([1, 2, 3]);

        // Act
        _ = stream.Encrypt([4, 5, 6]);

        // Assert
        stream.Position.ShouldBe(0);
    }
}
