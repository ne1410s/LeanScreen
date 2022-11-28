// <copyright file="ThumbingExtensionsTests.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace Av.Tests;

/// <summary>
/// Tests for the <see cref="ThumbingExtensions"/>.
/// </summary>
public class ThumbingExtensionsTests
{
    [Fact]
    public void DistributeEvenly_WithValue_ReturnsExpected()
    {
        // Arrange
        var value = TimeSpan.FromSeconds(10);
        var expected = new TimeSpan[] { TimeSpan.Zero, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(10) };

        // Act
        var result = value.DistributeEvenly(3);

        // Assert
        result.Should().BeEquivalentTo(expected);
    }
}
