// <copyright file="CollationOptions.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace Av.Imaging;

using System;
using System.Linq;
using Av.Common;

/// <summary>
/// Collation options.
/// </summary>
public class CollationOptions
{
    /// <summary>
    /// Gets or sets the total number of columns in the collation.
    /// </summary>
    public int Columns { get; set; } = 4;

    /// <summary>
    /// Gets or sets the number of pixels separating each column.
    /// </summary>
    public int SpaceX { get; set; } = 10;

    /// <summary>
    /// Gets or sets the number of pixels separating each row.
    /// </summary>
    public int SpaceY { get; set; } = 10;

    /// <summary>
    /// Gets or sets the number of pixels before the first column.
    /// </summary>
    public int Top { get; set; } = 120;

    /// <summary>
    /// Gets or sets the number of pixels after the last column.
    /// </summary>
    public int Bottom { get; set; } = 10;

    /// <summary>
    /// Gets or sets the number of pixels before the first row and after the
    /// last row.
    /// </summary>
    public int Sides { get; set; } = 10;

    /// <summary>
    /// Gets or sets the item size. If not supplied, the dimensions of the
    /// first frame is used. A single dimension is suggested, as this shall
    /// result in the original aspect ratio being preserved.
    /// </summary>
    public Size2D? ItemSize { get; set; }

    /// <summary>
    /// Generates a map to serve as instructions for how frames can be collated.
    /// </summary>
    /// <param name="itemSize">The dimensions of each item.</param>
    /// <param name="itemCount">The total number of items.</param>
    /// <returns>A collation map.</returns>
    public CollationMap GetMap(Size2D itemSize, int itemCount)
    {
        var rows = (int)Math.Ceiling((double)itemCount / this.Columns);
        var canvasSize = new Size2D
        {
            Width = (2 * this.Sides) + (this.Columns * itemSize.Width) + ((this.Columns - 1) * this.SpaceX),
            Height = this.Top + (rows * itemSize.Height) + ((rows - 1) * this.SpaceY) + this.Bottom,
        };

        var positions = Enumerable.Range(0, itemCount).Select(index =>
        {
            var gridCol = index % this.Columns;
            var gridRow = (int)Math.Floor((double)index / this.Columns);
            var x = this.Sides + (gridCol * itemSize.Width) + (gridCol * this.SpaceX);
            var y = this.Top + (gridRow * itemSize.Height) + (gridRow * this.SpaceY);
            return new Point2D(x, y);
        });

        return new(canvasSize, itemSize, positions.ToList());
    }
}
