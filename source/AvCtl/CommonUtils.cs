// <copyright file="CommonUtils.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace AvCtl;

using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Av.Abstractions.Rendering;
using Av.Rendering.Ffmpeg;
using Av.Services;
using Crypt;
using Crypt.Hashing;
using Crypt.IO;

/// <summary>
/// Common utilities.
/// </summary>
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
    /// Gets a thumbnail generator.
    /// </summary>
    /// <param name="source">The video source path.</param>
    /// <param name="key">The key, if a secure file.</param>
    /// <param name="renderer">The chosen renderer.</param>
    /// <returns>A thumbnail generator.</returns>
    public static ThumbnailGenerator GetSnapper(
        string source, byte[] key, out IRenderingService renderer)
    {
        renderer = GetRenderer(source, key);
        return new ThumbnailGenerator(renderer);
    }

    /// <summary>
    /// Gets a renderer.
    /// </summary>
    /// <param name="source">The video source path.</param>
    /// <param name="key">The key, if a secure file.</param>
    /// <returns>A renderer.</returns>
    public static IRenderingService GetRenderer(string source, byte[] key)
    {
        return new FfmpegRenderer(source, key);
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
