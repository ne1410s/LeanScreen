// <copyright file="CollationOptionsTests.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace LeanScreen.Abstractions.Tests.Imaging;

using LeanScreen.Common;
using LeanScreen.Imaging;

/// <summary>
/// Tests for the <see cref="CollationOptions"/>.
/// </summary>
public class CollationOptionsTests
{
    [Fact]
    public void GetMap_MultipleRows_ReturnsExpected()
    {
        // Arrange
        var sut = new CollationOptions { Columns = 4, Top = 50 };
        var itemSize = new Size2D(200, 150);
        var itemCount = (sut.Columns * 2) + 1;
        var expectedCoords = new List<Point2D>
        {
            new(10, 50),
            new(220, 50),
            new(430, 50),
            new(640, 50),
            new(10, 210),
            new(220, 210),
            new(430, 210),
            new(640, 210),
            new(10, 370),
        };

        // Act
        var result = sut.GetMap(itemSize, itemCount);

        // Assert
        result.ShouldBeEquivalentTo(new CollationMap(new(850, 530), itemSize, expectedCoords));
    }
}