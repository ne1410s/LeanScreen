// <copyright file="SixLaborsImagingService.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace Av.Imaging.SixLabors
{
    using System.IO;
    using System.Threading.Tasks;
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
            retVal.Position = 0;
            return retVal;
        }

        /// <inheritdoc/>
        public async Task<MemoryStream> ResizeImage(Stream stream, Size2D targetSize)
        {
            var format = await Image.DetectFormatAsync(stream);
            var image = await Image.LoadAsync(stream);
            image.Resize(targetSize);
            var retVal = new MemoryStream();
            await image.SaveAsync(retVal, format);
            return retVal;
        }
    }
}