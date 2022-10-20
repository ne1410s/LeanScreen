// <copyright file="CollationMap.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace Av.Abstractions.Imaging
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using Av.Abstractions.Rendering;
    using Av.Abstractions.Shared;

    /// <summary>
    /// A description of placeholder positions within the context of a canvas.
    /// </summary>
    public class CollationMap
    {
        /// <summary>
        /// Initialises a new instance of the <see cref="CollationMap"/> class.
        /// </summary>
        /// <param name="canvasSize">The canvas size.</param>
        /// <param name="itemSize">The item size.</param>
        /// <param name="coordinates">The (x, y) positions.</param>
        public CollationMap(
            Dimensions2D canvasSize,
            Dimensions2D itemSize,
            IList<Dimensions2D> coordinates)
        {
            this.CanvasSize = canvasSize;
            this.ItemSize = itemSize;
            this.Coordinates = coordinates;
        }

        /// <summary>
        /// Gets the canvas size.
        /// </summary>
        public Dimensions2D CanvasSize { get; }

        /// <summary>
        /// Gets the canvas size.
        /// </summary>
        public Dimensions2D ItemSize { get; }

        /// <summary>
        /// Gets the placeholder positions.
        /// </summary>
        public IList<Dimensions2D> Coordinates { get; }
    }
}
