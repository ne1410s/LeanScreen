// <copyright file="Dimensions2D.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace Av.Abstractions.Shared
{
    /// <summary>
    /// 2D dimensions.
    /// </summary>
    public struct Dimensions2D
    {
        /// <summary>
        /// Initialises a new instance of the <see cref="Dimensions2D"/> struct.
        /// </summary>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        public Dimensions2D(int width, int height)
        {
            this.Width = width;
            this.Height = height;
        }

        /// <summary>
        /// Gets or sets the height.
        /// </summary>
        public int Height { get; set; }

        /// <summary>
        /// Gets or sets the width.
        /// </summary>
        public int Width { get; set; }
    }
}
