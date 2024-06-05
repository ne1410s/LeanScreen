// <copyright file="FileExtensions.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace LeanScreen;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using CryptoStream.Encoding;
using CryptoStream.Hashing;
using CryptoStream.IO;
using CryptoStream.Keying;
using LeanScreen.Common;

/// <summary>
/// File extensions.
/// </summary>
public static class FileExtensions
{
    private const string AllFilesWildcard = "*";

    /// <summary>
    /// Gets a directory based on file system context.
    /// </summary>
    /// <param name="fi">The source media.</param>
    /// <param name="destination">The supplied destination, if provided.</param>
    /// <returns>Directory info.</returns>
    public static DirectoryInfo QualifyDestination(this FileInfo fi, string? destination)
    {
        if (destination == null && fi?.Exists == true)
        {
            destination = fi.DirectoryName;
        }

        return new DirectoryInfo(destination ?? Directory.GetCurrentDirectory());
    }

    /// <summary>
    /// Enumerates all media files recursively, according to the specified
    /// media type(s).
    /// </summary>
    /// <param name="di">The root directory info.</param>
    /// <param name="mediaTypes">The media type(s) to look for.</param>
    /// <param name="secure">Whether to look only for secure or non-secure
    /// items. If null, both these states are included.</param>
    /// <param name="recurse">Whether to recurse.</param>
    /// <param name="skip">Files to skip.</param>
    /// <param name="take">Files to take.</param>
    /// <returns>A sequence of file paths.</returns>
    public static IEnumerable<FileInfo> EnumerateMedia(
        this DirectoryInfo di,
        MediaTypes mediaTypes,
        bool? secure = null,
        bool recurse = false,
        int skip = 0,
        int take = int.MaxValue)
    {
        di = di ?? throw new ArgumentNullException(nameof(di));
        var searchOpt = recurse ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
        return di
            .EnumerateFiles(AllFilesWildcard, searchOpt)
            .Where(fi => (secure == null || fi.IsSecure() == secure)
                && mediaTypes.HasFlag(fi.GetMediaTypeInfo().MediaType))
            .Skip(skip)
            .Take(take);
    }

    /// <summary>
    /// Makes a key.
    /// </summary>
    /// <param name="keySource">Key file source.</param>
    /// <param name="keyRegex">Key file regex.</param>
    /// <param name="entropy">The entropy.</param>
    /// <param name="checkSum">The check sum.</param>
    /// <returns>The key.</returns>
    public static ReadOnlyMemory<byte> MakeKey(
        this DirectoryInfo? keySource,
        Regex? keyRegex,
        IEnumerable<string> entropy,
        out string checkSum)
    {
        var blended = entropy.Blend();
        var hashes = GetHashes(keySource, keyRegex);
        var key = new DefaultKeyDeriver().DeriveKey(blended, hashes);
        checkSum = key.Hash(HashType.Md5).Encode(Codec.ByteBase64);
        return key;
    }

    private static string Blend(this IEnumerable<string> input)
    {
        input ??= [];
        var primary = input.FirstOrDefault() ?? string.Empty;
        var remaining = string.Concat(input.Skip(1));
        var sb = new StringBuilder();
        for (var r = 0; r < Math.Max(primary.Length, remaining.Length); r++)
        {
            sb.Append(primary[r % primary.Length]);
            if (remaining.Length != 0)
            {
                sb.Append(remaining[r % remaining.Length]);
            }
        }

        return sb.ToString();
    }

    private static byte[][] GetHashes(DirectoryInfo? keySource, Regex? keyRegex)
    {
        if (keySource == null)
        {
            return [];
        }

        if (!keySource.Exists)
        {
            throw new ArgumentException($"Directory not found: {keySource}", nameof(keySource));
        }

        return keySource.EnumerateFiles()
            .Where(f => keyRegex?.IsMatch(f.Name) != false)
            .Select(f => f.Hash(HashType.Sha1))
            .ToArray();
    }
}
