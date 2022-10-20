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
    using global::SixLabors.ImageSharp;
    using global::SixLabors.ImageSharp.PixelFormats;

    /// <inheritdoc cref="ICollatingService"/>
    public class SixLaborsCollatingService : ICollatingService
    {
        /// <inheritdoc/>
        public MemoryStream Collate(IEnumerable<RenderedFrame> frames, CollationOptions opts = null)
        {
            opts ??= new CollationOptions();
            IEnumerable<RenderedFrame> orderedFrames = opts.ForceChronology
                ? frames.OrderBy(f => f.FrameNumber)
                : frames;

            var map = opts.GetMap(orderedFrames.First().Dimensions, frames.Count());
            var canvas = new Image<Rgb24>(map.CanvasSize.Width, map.CanvasSize.Height);
            var iterIndex = 0;
            foreach (var frame in orderedFrames)
            {
                var coords = map.Coordinates[iterIndex++];
                Console.WriteLine($"TODO: Frame {frame.FrameNumber} at coords ({coords.X}, {coords.Y})");
            }

            throw new NotImplementedException();
        }
    }
}
