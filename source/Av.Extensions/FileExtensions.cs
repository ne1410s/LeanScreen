﻿// <copyright file="FileExtensions.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace Av.Extensions;

using System.IO;
using Av.Imaging.SixLabors;
using Av.Rendering;
using Av.Rendering.Ffmpeg;
using Av.Snaps;
using Crypt.IO;

/// <summary>
/// Extensions for files.
/// </summary>
public static class FileExtensions
{
    private static readonly SnapService Snapper = new(
        new FfmpegRendererFactory(),
        new SixLaborsImagingService());

    /// <summary>
    /// Gets media info.
    /// </summary>
    /// <param name="fi">The file.</param>
    /// <param name="key">The Key.</param>
    /// <returns>Media info.</returns>
    public static MediaInfo GetMediaInfo(this FileInfo fi, byte[] key)
    {
        using var str = fi.NotNull(nameof(fi)).OpenRead();
        return Snapper.GetInfo(str, fi.IsSecure() ? fi.ToSalt() : [], key);
    }

    /// <summary>
    /// Extracts a single frame to an unencrypted image stream.
    /// </summary>
    /// <param name="fi">The file.</param>
    /// <param name="key">The key.</param>
    /// <param name="position">The relative position.</param>
    /// <param name="height">The desired height in pixels.</param>
    /// <returns>Image stream.</returns>
    public static MemoryStream Snap(this FileInfo fi, byte[] key, double position = 0.4, int? height = 300)
    {
        using var str = fi.NotNull(nameof(fi)).OpenRead();
        return Snapper.Snap(str, fi.IsSecure() ? fi.ToSalt() : [], key, position, height);
    }

    /// <summary>
    /// Extracts a single frame and saves in an adjacent file.
    /// </summary>
    /// <param name="fi">The file.</param>
    /// <param name="key">The key.</param>
    /// <param name="position">The relative position.</param>
    /// <param name="height">The desired height in pixels.</param>
    /// <returns>The file location.</returns>
    public static string SnapHere(this FileInfo fi, byte[] key, double position = 0.4, int? height = 300)
    {
        var fileName = fi.NotNull(nameof(fi)).Name;
        var secure = fi.IsSecure();
        using var str = fi.Snap(key, position, height);
        var formatHeight = ((long)(height ?? 0)).FormatToUpperBound(9999);
        var formatPosition = ((long)(position * 100)).FormatToUpperBound(100);
        var nameToUse = $"{fileName}_snap_p{formatPosition}_h{formatHeight}.jpg";
        if (secure)
        {
            var newSalt = str.Encrypt(key);
            nameToUse = fileName.Substring(0, 12) + "." + newSalt + ".jpg";
        }

        var targetPath = Path.Combine(fi.NotNull(nameof(fi)).Directory.FullName, nameToUse);
        using var ss = File.OpenWrite(targetPath);
        str.CopyTo(ss);
        return targetPath;
    }

    /// <summary>
    /// Combines multiple frames to a single unencrypted image stream.
    /// </summary>
    /// <param name="fi">The file.</param>
    /// <param name="key">The key.</param>
    /// <param name="total">The total number of frames.</param>
    /// <param name="columns">The number of columns.</param>
    /// <param name="height">The desired height in pixels.</param>
    /// <returns>Image stream.</returns>
    public static MemoryStream Collate(this FileInfo fi, byte[] key, int total = 24, int columns = 4, int? height = 300)
    {
        using var str = fi.NotNull(nameof(fi)).OpenRead();
        return Snapper.Collate(str, fi.IsSecure() ? fi.ToSalt() : [], key, total, columns, height);
    }

    /// <summary>
    /// Combines multiple frames to a single image and saves in an adjacent file.
    /// </summary>
    /// <param name="fi">The file.</param>
    /// <param name="key">The key.</param>
    /// <param name="total">The total number of frames.</param>
    /// <param name="columns">The number of columns.</param>
    /// <param name="height">The desired height in pixels.</param>
    /// <returns>The target path.</returns>
    public static string CollateHere(this FileInfo fi, byte[] key, int total = 24, int columns = 4, int? height = 300)
    {
        var fileName = fi.NotNull(nameof(fi)).Name;
        var secure = fi.IsSecure();
        using var str = fi.Collate(key, total, columns, height);
        var formatTotal = ((long)total).FormatToUpperBound(999);
        var formatColumns = ((long)columns).FormatToUpperBound(99);
        var formatHeight = ((long)(height ?? 0)).FormatToUpperBound(9999);
        var nameToUse = $"{fileName}_collate_t{formatTotal}_c{formatColumns}_h{formatHeight}.jpg";
        if (secure)
        {
            var newSalt = str.Encrypt(key);
            nameToUse = fileName.Substring(0, 12) + "." + newSalt + ".jpg";
        }

        var targetPath = Path.Combine(fi.NotNull(nameof(fi)).Directory.FullName, nameToUse);
        using var ss = File.OpenWrite(targetPath);
        str.CopyTo(ss);
        return targetPath;
    }
}
