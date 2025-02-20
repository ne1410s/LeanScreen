﻿// <copyright file="IImagingService.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace LeanScreen.Imaging;

using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using LeanScreen.Common;
using LeanScreen.Rendering;

/// <summary>
/// Implementation for image services.
/// </summary>
public interface IImagingService
{
    /// <summary>
    /// Gets the natural image size.
    /// </summary>
    /// <param name="stream">The stream.</param>
    /// <returns>The dimensions.</returns>
    Task<Size2D> GetSize(Stream stream);

    /// <summary>
    /// Encodes image bytes to a stream.
    /// </summary>
    /// <param name="rgb24Bytes">The image bytes as 24-bit rgb.</param>
    /// <param name="size">The image dimensions.</param>
    /// <returns>A stream of encoded image bytes.</returns>
    MemoryStream Encode(byte[] rgb24Bytes, Size2D size);

    /// <summary>
    /// Resizes an image to the target size.
    /// </summary>
    /// <param name="stream">The original image stream.</param>
    /// <param name="targetSize">The target size.</param>
    /// <returns>A stream of encoded image bytes.</returns>
    Task<MemoryStream> ResizeImage(Stream stream, Size2D targetSize);

    /// <summary>
    /// Collates a sequence of frames.
    /// </summary>
    /// <param name="frames">The frames.</param>
    /// <param name="opts">Collation options.</param>
    /// <returns>A stream of encoded image bytes.</returns>
    MemoryStream Collate(IEnumerable<RenderedFrame> frames, CollationOptions? opts = null);
}
