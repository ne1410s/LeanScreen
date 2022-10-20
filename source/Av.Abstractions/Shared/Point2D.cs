// <copyright file="Point2D.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace Av.Abstractions.Shared
{
    /// <summary>
    /// 2D point.
    /// </summary>
    public struct Point2D
    {
        /// <summary>
        /// Initialises a new instance of the <see cref="Point2D"/> struct.
        /// </summary>
        /// <param name="x">The horizontal coordinate.</param>
        /// <param name="y">The vertical coordinate.</param>
        public Point2D(int x, int y)
        {
            this.X = x;
            this.Y = y;
        }

        /// <summary>
        /// Gets or sets the horizontal coordinate.
        /// </summary>
        public int X { get; set; }

        /// <summary>
        /// Gets or sets the vertical coordinate.
        /// </summary>
        public int Y { get; set; }
    }
}
