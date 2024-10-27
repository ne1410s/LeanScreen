// <copyright file="FfmpegFormatConverter.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace LeanScreen.Rendering.Ffmpeg.Conversion;

using System.IO;
using CryptoStream.Streams;
using LeanScreen.Abstractions.Conversion;

/// <inheritdoc/>
public class FfmpegFormatConverter : IFormatConverter
{
    /// <inheritdoc/>
    public Stream Remux(ISimpleReadStream source, MediaInfo sourceInfo)
    {

    }
}
