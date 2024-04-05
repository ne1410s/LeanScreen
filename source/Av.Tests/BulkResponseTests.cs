// <copyright file="BulkResponseTests.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace Av.Tests;

using Av.BulkProcess;

/// <summary>
/// Tests for the <see cref="BulkResponse"/> class.
/// </summary>
public class BulkResponseTests
{
    [Theory]
    [InlineData(20, 20)]
    [InlineData(40, 40)]
    public void Ctor_VaryingProgress_CalculatesPercent(int processed, double expected)
    {
        // Arrange & Act
        var result = new BulkResponse(100) { Processed = processed };

        // Assert
        result.Percent.Should().Be(expected);
    }
}
