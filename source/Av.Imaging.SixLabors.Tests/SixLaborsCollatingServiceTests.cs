using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        var sut = new SixLaborsCollatingService();
        var bareFrame = new RenderedFrame { Dimensions = new(200, 150) };
        sut.Frames.AddRange(Enumerable.Repeat(bareFrame, 9));

        // Act
        sut.RenderCollation();

        // Assert
    }
}
