// <copyright file="SixLaborsCollatingService.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace Av.Imaging.SixLabors
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Av.Abstractions.Imaging;
    using Av.Abstractions.Rendering;
    using Av.Abstractions.Shared;

    /// <inheritdoc/>
    public class SixLaborsCollatingService : ICollatingService
    {
        /// <inheritdoc/>
        public List<RenderedFrame> Frames { get; } = new();

        /// <inheritdoc/>
        public MemoryStream RenderCollation(CollationOptions opts = null)
        {
            // TODO!!
            // All the following logic can and should be centralised!! 
            // Somehow put in Av library (e.g. extension method(s) or single service, etc)?
            // It is core logic, nothing to do with SixLabors, and should be available for reuse!

            opts ??= new CollationOptions();

            var count = this.Frames.Count;
            IEnumerable<RenderedFrame> frames = opts.ForceChronology
                ? this.Frames.OrderBy(f => f.FrameNumber)
                : this.Frames;
            var itemWidth = frames.First().Dimensions.Width;
            var itemHeight = frames.First().Dimensions.Height;
            var rows = (int)Math.Ceiling((double)count / opts.Columns);
            var canvasDimensions = new Dimensions2D
            {
                Width = (2 * opts.Sides) + (opts.Columns * itemWidth) + ((opts.Columns - 1) * opts.SpaceX),
                Height = opts.Top + (rows * itemHeight) + ((rows - 1) * opts.SpaceY) + opts.Bottom,
            };

            // x, y
            var test = new List<Tuple<int, int>>();
            var iterNo = 0;
            foreach (var f in frames)
            {
                var gridCol = iterNo % opts.Columns;
                var gridRow = (int)Math.Floor((double)iterNo / opts.Columns);
                var x = opts.Sides + (gridCol * itemWidth) + (gridCol * opts.SpaceX);
                var y = opts.Top + (gridRow * itemHeight) + (gridRow * opts.SpaceY);
                test.Add(new(x, y));
                iterNo++;
            }

            throw new NotImplementedException();
        }
    }
}
