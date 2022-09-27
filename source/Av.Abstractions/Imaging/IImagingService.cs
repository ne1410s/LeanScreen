using System.IO;
using Av.Abstractions.Shared;

namespace Av.Abstractions.Imaging
{
    /// <summary>
    /// Implementation for image services.
    /// </summary>
    public interface IImagingService
    {
        /// <summary>
        /// Encodes image bytes to a stream.
        /// </summary>
        /// <param name="rgb24Bytes">The image bytes as 24-bit rgb.</param>
        /// <param name="width">The image width.</param>
        /// <param name="height">The image height.</param>
        /// <returns>A stream of encoded bytes.</returns>
        Stream Encode(byte[] rgb24Bytes, Dimensions2D size);
    }
}
