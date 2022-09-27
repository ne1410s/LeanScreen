using System.IO;
using Av.Abstractions.Imaging;
using Av.Abstractions.Shared;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.PixelFormats;

namespace Av.Imaging.SixLabors
{
    /// <inheritdoc cref="IImagingService"/>
    public class SixLaborsImagingService : IImagingService
    {
        /// <inheritdoc/>
        public Stream Encode(byte[] rgb24Bytes, Dimensions2D size)
        {
            var image = Image.LoadPixelData<Rgb24>(rgb24Bytes, size.Width, size.Height);
            var retVal = new MemoryStream();
            image.Save(retVal, new JpegEncoder());
            return retVal;
        }
    }
}