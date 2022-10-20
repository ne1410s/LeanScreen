// <copyright file="SixLaborsImagingService.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace Av.Imaging.SixLabors
{
    using System.IO;
    using Av.Abstractions.Imaging;
    using Av.Abstractions.Shared;
    using global::SixLabors.ImageSharp;
    using global::SixLabors.ImageSharp.Formats.Jpeg;
    using global::SixLabors.ImageSharp.PixelFormats;

    /// <inheritdoc cref="IImagingService"/>
    public class SixLaborsImagingService : IImagingService
    {
        /// <inheritdoc/>
        public MemoryStream Encode(byte[] rgb24Bytes, Size2D size)
        {
            var image = Image.LoadPixelData<Rgb24>(rgb24Bytes, size.Width, size.Height);
            var retVal = new MemoryStream();
            image.Save(retVal, new JpegEncoder());
            return retVal;
        }
    }
}