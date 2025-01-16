// <copyright file="IFormatConverter.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace LeanScreen.Abstractions.Conversion;

using System.IO;

/// <summary>
/// Handles format conversion.
/// </summary>
public interface IFormatConverter
{
    /// <summary>
    /// Remultiplexes media into new format container (ie. not transcoded).
    /// </summary>
    /// <param name="source">The source.</param>
    /// <param name="ext">The target extension.</param>
    /// <param name="key">The key, if required.</param>
    /// <param name="directFile">If true then ffmpeg is sent only file path(s)
    /// from which format contexts are populated, instead of streams.</param>
    /// <param name="deleteSource">If true and the process succeeds, the source
    /// file is deleted.</param>
    /// <returns>The output file.</returns>
    public FileInfo Remux(
        FileInfo source, string ext, byte[] key, bool directFile = false, bool deleteSource = false);

    /// <summary>
    /// Fully transcodes a media into a new format. This is a more intensive
    /// operation that simple remux.
    /// </summary>
    /// <param name="source">The source.</param>
    /// <param name="ext">The target extension.</param>
    /// <param name="key">The key, if required.</param>
    /// <param name="directFile">If true then ffmpeg is sent only file path(s)
    /// from which format contexts are populated, instead of streams.</param>
    /// <param name="deleteSource">If true and the process succeeds, the source
    /// file is deleted.</param>
    /// <returns>The output file.</returns>
    public FileInfo Transcode(
        FileInfo source, string ext, byte[] key, bool directFile = false, bool deleteSource = false);
}
