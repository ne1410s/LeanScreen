// <copyright file="FormatExtensions.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace Av;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using Av.Common;
using CryptoStream.IO;
using CryptoStream.Transform;

/// <summary>
/// Extensions for formatting.
/// </summary>
[System.Diagnostics.CodeAnalysis.SuppressMessage(
    "Major Code Smell",
    "S3358:Ternary operators should not be nested",
    Justification = "Sorry, Mom")]
public static class FormatExtensions
{
    private const double Kilobyte = 1024;
    private const double Megabyte = 1048576;
    private const double Gigabyte = 1073741824;
    private const double Terabyte = 1099511627776;

    private static readonly ReadOnlyDictionary<string, string> ArchiveMimes =
        new(new Dictionary<string, string>()
        {
            { ".7z", "application/x-7z-compressed" },
            { ".gz", "application/gzip" },
            { ".jar", "application/java-archive" },
            { ".rar", "application/vnd.rar" },
            { ".tar", "application/x-tar" },
            { ".zip", "application/zip" },
        });

    private static readonly ReadOnlyDictionary<string, string> ImageMimes =
        new(new Dictionary<string, string>
        {
            { ".bmp", "image/bmp" },
            { ".gif", "image/gif" },
            { ".jpe", "image/jpeg" },
            { ".jpeg", "image/jpeg" },
            { ".jpg", "image/jpeg" },
            { ".png", "image/png" },
            { ".tif", "image/tiff" },
            { ".tiff", "image/tiff" },
            { ".webp", "image/webp" },
        });

    private static readonly ReadOnlyDictionary<string, string> AudioMimes =
        new(new Dictionary<string, string>
        {
            { ".aac", "audio/aac" },
            { ".m4a", "audio/m4a" },
            { ".mp3", "audio/mpeg" },
            { ".oga", "audio/ogg" },
            { ".wav", "audio/wav" },
            { ".weba", "audio/webm" },
        });

    private static readonly ReadOnlyDictionary<string, string> VideoMimes =
        new(new Dictionary<string, string>
        {
            { ".3g2", "video/3gpp2" },
            { ".3gp", "video/3gpp" },
            { ".avi", "video/x-msvideo" },
            { ".flv", "video/x-flv" },
            { ".m2v", "video/mpeg" },
            { ".m4v", "video/x-m4v" },
            { ".mkv", "video/x-matroska" },
            { ".mov", "video/quicktime" },
            { ".mp4", "video/mp4" },
            { ".mpeg", "video/mpeg" },
            { ".mpg", "video/mpeg" },
            { ".mts", "video/mp2t" },
            { ".swf", "application/x-shockwave-flash" },
            { ".ts", "video/mp2t" },
            { ".ogg", "video/ogg" },
            { ".ogv", "video/ogg" },
            { ".vob", "video/x-ms-vob" },
            { ".webm", "video/webm" },
            { ".wmv", "video/x-ms-wmv" },
        });

    /// <summary>
    /// Formats a byte count as size on disk.
    /// </summary>
    /// <param name="bytes">The byte count.</param>
    /// <returns>Size on disk.</returns>
    public static string FormatSize(this long bytes) =>
        bytes < Kilobyte ? $"{bytes}b"
            : bytes < Megabyte ? $"{bytes / Kilobyte:N0}kb"
            : bytes < Gigabyte ? $"{bytes / Megabyte:N0}mb"
            : bytes < Terabyte ? $"{bytes / Gigabyte:N0}gb"
            : $"{bytes / Terabyte:N0}tb";

    /// <summary>
    /// Gets an upper-bound format; which can be used to format shorter numbers
    /// so that they are left-padded with the appropriate number of zeros.
    /// </summary>
    /// <typeparam name="T">The upper bounded type.</typeparam>
    /// <param name="upperBound">The upper bound.</param>
    /// <returns>A format string.</returns>
    public static string GetUpperBoundFormat<T>(this T upperBound)
        where T : struct
            => "D" + $"{upperBound}".Length;

    /// <summary>
    /// Formats the value, left-padding with zeros to the length of the upper bound.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="upperBound">The upper bound.</param>
    /// <returns>The formatted value.</returns>
    public static string FormatToUpperBound(this long value, long upperBound)
        => value.ToString(upperBound.GetUpperBoundFormat(), CultureInfo.InvariantCulture);

    /// <summary>
    /// Provides a format string sufficient to house the largest unit of
    /// either hours, minutes or seconds in the supplied time span.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns>The concise format.</returns>
    public static string GetConciseFormat(this TimeSpan value)
        => value.TotalDays >= 1
            ? @"d\.hh\:mm\:ss"
            : value.TotalHours >= 1
                ? @"h\:mm\:ss"
                : value.TotalMinutes >= 1
                    ? @"m\:ss"
                    : @"s\.f\s";

    /// <summary>
    /// Concisely formats a timespan.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns>The concise text.</returns>
    public static string FormatConcise(this TimeSpan value)
        => value.ToString(value.GetConciseFormat(), CultureInfo.InvariantCulture);

    /// <summary>
    /// Gets apparent media type information based on a file extension.
    /// </summary>
    /// <param name="fi">The file info.</param>
    /// <param name="decryptor">Optional decryptor.</param>
    /// <returns>Media type information.</returns>
    public static MediaTypeInfo GetMediaTypeInfo(this FileInfo fi, IGcmDecryptor? decryptor = null)
    {
        decryptor ??= new AesGcmDecryptor();
        var extension = fi.NotNull().IsSecure()
            ? fi.ToPlainExtension(decryptor)
            : '.' + fi.Extension.ToLowerInvariant().TrimStart('.');

        return ArchiveMimes.TryGetValue(extension, out var arcMime) ? new MediaTypeInfo(MediaTypes.Archive, arcMime)
            : ImageMimes.TryGetValue(extension, out var imgMime) ? new MediaTypeInfo(MediaTypes.Image, imgMime)
            : AudioMimes.TryGetValue(extension, out var audMime) ? new MediaTypeInfo(MediaTypes.Audio, audMime)
            : VideoMimes.TryGetValue(extension, out var vidMime) ? new MediaTypeInfo(MediaTypes.Video, vidMime)
            : new MediaTypeInfo(MediaTypes.NonMedia, null!);
    }

    /// <summary>
    /// Gets a set of supported extensions for a given media type.
    /// </summary>
    /// <param name="mediaType">The media type.</param>
    /// <returns>A set of file extensions.</returns>
    public static HashSet<string> GetExtensions(this MediaTypes mediaType)
    {
        var extensions = new List<string>();
        if (mediaType.HasFlag(MediaTypes.Archive))
        {
            extensions.AddRange(ArchiveMimes.Keys);
        }

        if (mediaType.HasFlag(MediaTypes.Image))
        {
            extensions.AddRange(ImageMimes.Keys);
        }

        if (mediaType.HasFlag(MediaTypes.Audio))
        {
            extensions.AddRange(AudioMimes.Keys);
        }

        if (mediaType.HasFlag(MediaTypes.Video))
        {
            extensions.AddRange(VideoMimes.Keys);
        }

        return new HashSet<string>(extensions, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Gets an evenly-distributed sequence of times.
    /// </summary>
    /// <param name="duration">The total duration.</param>
    /// <param name="count">The number of items to distribute.</param>
    /// <returns>A sequence of evenly-distributed times.</returns>
    public static TimeSpan[] DistributeEvenly(this TimeSpan duration, int count)
    {
        var deltaMs = duration.TotalMilliseconds / (Math.Max(2, count) - 1);
        return Enumerable.Range(0, count)
            .Select(n => TimeSpan.FromMilliseconds(deltaMs * n))
            .ToArray();
    }
}
