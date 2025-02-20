﻿// <copyright file="CollationMapTests.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace LeanScreen.Abstractions.Tests.Imaging;

using LeanScreen.Common;
using LeanScreen.Imaging;

/// <summary>
/// Tests for the <see cref="CollationMap"/>.
/// </summary>
public class CollationMapTests
{
    [Fact]
    public void Ctor_WithSize_RetainsValue()
    {
        // Arrange
        var canvasSize = new Size2D(2, 1);
        var itemSize = new Size2D(4, 6);
        var coordinates = new Point2D[] { new(1, 2), new(12, 6) };

        // Act
        var sut = new CollationMap(canvasSize, itemSize, coordinates);

        // Assert
        sut.CanvasSize.ShouldBe(canvasSize);
        sut.ItemSize.ShouldBe(itemSize);
        sut.Coordinates.ShouldBeEquivalentTo(coordinates);
    }
}
