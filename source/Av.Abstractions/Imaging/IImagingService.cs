// <copyright file="IImagingService.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace Av.Abstractions.Imaging
{
    using System.IO;
    using Av.Abstractions.Shared;

    /// <summary>
    /// Implementation for image services.
    /// </summary>
    public interface IImagingService
    {
        /// <summary>
        /// Encodes image bytes to a stream.
        /// </summary>
        /// <param name="rgb24Bytes">The image bytes as 24-bit rgb.</param>
        /// <param name="size">The image dimensions.</param>
        /// <returns>A stream of encoded bytes.</returns>
        MemoryStream Encode(byte[] rgb24Bytes, Dimensions2D size);
    }
}
