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
    using global::SixLabors.ImageSharp;
    using global::SixLabors.ImageSharp.Formats.Jpeg;
    using global::SixLabors.ImageSharp.PixelFormats;
    using global::SixLabors.ImageSharp.Processing;

    /// <inheritdoc cref="ICollatingService"/>
    public class SixLaborsCollatingService : ICollatingService
    {
        /// <inheritdoc/>
        public MemoryStream Collate(IEnumerable<RenderedFrame> frames, CollationOptions opts = null)
        {
            opts ??= new CollationOptions();
            var map = opts.GetMap(frames.First().Dimensions, frames.Count());
            var canvas = new Image<Rgb24>(map.CanvasSize.Width, map.CanvasSize.Height);
            var iterIndex = 0;
            foreach (var frame in frames)
            {
                var inSize = frame.Dimensions;
                var item = Image.LoadPixelData<Rgb24>(frame.Rgb24Bytes, inSize.Width, inSize.Height);
                var coords = map.Coordinates[iterIndex++];
                item.Resize(map.ItemSize);
                canvas.Mutate(o => o.DrawImage(item, new Point(coords.X, coords.Y), 1));
            }

            var retVal = new MemoryStream();
            canvas.Save(retVal, new JpegEncoder());
            retVal.Position = 0;
            return retVal;
        }
    }
}
