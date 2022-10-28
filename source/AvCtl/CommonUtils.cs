﻿// <copyright file="CommonUtils.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace AvCtl;

using System.Globalization;
using System.Linq;
using Av.Abstractions.Rendering;
using Av.Rendering.Ffmpeg;
using Av.Services;

/// <summary>
/// Common utilities.
/// </summary>
internal static class CommonUtils
{
    /// <summary>
    /// Gets a directory based on user-supplied parameters.
    /// </summary>
    /// <param name="source">The source media.</param>
    /// <param name="destination">The supplied destination, if provided.</param>
    /// <returns>Directory info.</returns>
    public static DirectoryInfo QualifyDestination(string source, string? destination)
    {
        var fi = new FileInfo(source);
        if (destination == null && fi.Exists)
        {
            destination = fi.DirectoryName;
        }

        return new DirectoryInfo(destination ?? Directory.GetCurrentDirectory());
    }

    /// <summary>
    /// Gets a thumbnail generator.
    /// </summary>
    /// <param name="source">The video source path.</param>
    /// <param name="keyCsv">The key csv, if a secure file.</param>
    /// <param name="renderer">The chosen renderer.</param>
    /// <param name="key">The key.</param>
    /// <returns>A thumbnail generator.</returns>
    public static ThumbnailGenerator GetSnapper(
        string source, string? keyCsv, out IRenderingService renderer, out byte[]? key)
    {
        renderer = GetRenderer(source, keyCsv, out key);
        return new ThumbnailGenerator(renderer);
    }

    /// <summary>
    /// Gets a renderer.
    /// </summary>
    /// <param name="source">The video source path.</param>
    /// <param name="keyCsv">The key csv, if a secure file.</param>
    /// <param name="key">The key.</param>
    /// <returns>A renderer.</returns>
    public static IRenderingService GetRenderer(string source, string? keyCsv, out byte[]? key)
    {
        key = GetKey(keyCsv);
        return new FfmpegRenderer(source, key);
    }

    /// <summary>
    /// Gets a key.
    /// </summary>
    /// <param name="keyCsv">The key csv, or null if no key.</param>
    /// <returns>The key bytes.</returns>
    public static byte[]? GetKey(string? keyCsv)
        => keyCsv?.Split(',').Select(b => byte.Parse(b, CultureInfo.InvariantCulture)).ToArray();
}