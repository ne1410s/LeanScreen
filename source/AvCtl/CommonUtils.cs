// <copyright file="CommonUtils.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace AvCtl;

using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Av.Abstractions.Rendering;
using Av.Abstractions.Shared;
using Av.Imaging.SixLabors;
using Av.Rendering.Ffmpeg;
using Av.Services;
using Comanche.Attributes;
using Crypt;
using Crypt.Hashing;
using Crypt.IO;

/// <summary>
/// Common utilities.
/// </summary>
[Hidden]
public static class CommonUtils
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
    /// Gets a renderer.
    /// </summary>
    /// <returns>A renderer.</returns>
    public static IRenderingService GetRenderer() => new FfmpegRenderer();

    /// <summary>
    /// Gets a capper.
    /// </summary>
    /// <param name="filePath">The file path.</param>
    /// <param name="key">The key.</param>
    /// <param name="thumbSize">The thumb size.</param>
    /// <returns>A capper.</returns>
    [SuppressMessage(
        "Reliability",
        "CA2000:Dispose objects before losing scope",
        Justification = "Disposed by owner")]
    public static Capper GetCapper(string filePath, byte[] key, Size2D? thumbSize = null)
    {
        var renderer = GetRenderer();
        renderer.SetSource(filePath, key, thumbSize);
        return new(renderer, new SixLaborsImagingService());
    }

    /// <summary>
    /// Blends strings.
    /// </summary>
    /// <param name="strings">The strings to blend.</param>
    /// <returns>A blended string.</returns>
    public static string Blend(this IEnumerable<string> strings)
    {
        strings ??= Array.Empty<string>();
        var primary = strings.FirstOrDefault() ?? string.Empty;
        var remaining = string.Concat(strings.Skip(1));
        var sb = new StringBuilder();
        for (var r = 0; r < Math.Max(primary.Length, remaining.Length); r++)
        {
            sb.Append(primary[r % primary.Length]);
            if (remaining.Length > 0)
            {
                sb.Append(remaining[r % remaining.Length]);
            }
        }

        return sb.ToString();
    }

    /// <summary>
    /// Gets Sha1 file hashes from a directory.
    /// </summary>
    /// <param name="sourceDir">Source directory.</param>
    /// <param name="sourceRegex">Source file regex.</param>
    /// <returns>A sequence of hashes.</returns>
    /// <exception cref="ArgumentException">Directory not found.</exception>
    public static byte[][] GetHashes(string? sourceDir, string? sourceRegex)
    {
        if (sourceDir == null)
        {
            return Array.Empty<byte[]>();
        }

        var di = new DirectoryInfo(sourceDir);
        if (!di.Exists)
        {
            throw new ArgumentException($"Directory not found: {sourceDir}", nameof(sourceDir));
        }

        return di.EnumerateFiles()
            .Where(f => sourceRegex == null || Regex.IsMatch(f.Name, sourceRegex))
            .Select(f => f.Hash(HashType.Sha1))
            .ToArray();
    }
}
