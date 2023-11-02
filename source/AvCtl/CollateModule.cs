// <copyright file="CollateModule.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace AvCtl;

using System.Globalization;
using System.IO;
using Av;
using Av.Abstractions.Imaging;
using Comanche.Attributes;
using Comanche.Services;
using Crypt.Encoding;
using Crypt.Hashing;
using Crypt.IO;
using Crypt.Keying;
using Crypt.Transform;

/// <summary>
/// Collate module.
/// </summary>
[Module("collate")]
public static class CollateModule
{
    /// <summary>
    /// Generates frames from a video source into a collated image.
    /// </summary>
    /// <param name="writer">Output writer.</param>
    /// <param name="source">The source file.</param>
    /// <param name="keySource">The key source directory.</param>
    /// <param name="keyRegex">The key source regular expression.</param>
    /// <param name="destination">The output folder.</param>
    /// <param name="itemCount">The total number of items.</param>
    /// <param name="columns">The number of columns to use.</param>
    /// <param name="itemHeight">The item height to set.</param>
    /// <returns>The output path.</returns>
    [Alias("evenly")]
    public static string CollateEvenly(
        [Hidden] IOutputWriter writer,
        [Alias("s")] string source,
        [Alias("ks")] string? keySource = null,
        [Alias("kr")] string? keyRegex = null,
        [Alias("d")] string? destination = null,
        [Alias("t")] int itemCount = 24,
        [Alias("c")] int columns = 4,
        [Alias("h")] int itemHeight = 300)
    {
        _ = writer ?? throw new ArgumentNullException(nameof(writer));
        var blendedInput = writer.CaptureStrings().Blend();
        var hashes = CommonUtils.GetHashes(keySource, keyRegex);
        var key = new DefaultKeyDeriver().DeriveKey(blendedInput, hashes);
        var md5Base64 = key.Hash(HashType.Md5).Encode(Codec.ByteBase64);

        writer.WriteLine($"Keys: {hashes.Length}, Check: {md5Base64}");
        var di = CommonUtils.QualifyDestination(source, destination);
        return CollateInternal(source, key, di.FullName, itemCount, columns, itemHeight);
    }

    /// <summary>
    /// Collates all video files in source.
    /// </summary>
    /// <param name="writer">Output writer.</param>
    /// <param name="source">The source directory.</param>
    /// <param name="keySource">The key source directory.</param>
    /// <param name="keyRegex">The key source regular expression.</param>
    /// <param name="destination">The output folder.</param>
    /// <param name="itemCount">The total number of items.</param>
    /// <param name="columns">The number of columns to use.</param>
    /// <param name="itemHeight">The item height to set.</param>
    /// <param name="recurse">Whether to recurse.</param>
    /// <param name="maxFiles">The maximum number of files to process.</param>
    [Alias("bulk")]
    public static void CollateManyEvenly(
        [Hidden] IOutputWriter writer,
        [Alias("s")] string source,
        [Alias("ks")] string? keySource = null,
        [Alias("kr")] string? keyRegex = null,
        [Alias("d")] string? destination = null,
        [Alias("t")] int itemCount = 24,
        [Alias("c")] int columns = 4,
        [Alias("h")] int itemHeight = 300,
        [Alias("r")] bool recurse = true,
        [Alias("m")] int maxFiles = 100)
    {
        _ = writer ?? throw new ArgumentNullException(nameof(writer));
        var blendedInput = writer.CaptureStrings().Blend();
        var hashes = CommonUtils.GetHashes(keySource, keyRegex);
        var key = new DefaultKeyDeriver().DeriveKey(blendedInput, hashes);
        var md5Base64 = key.Hash(HashType.Md5).Encode(Codec.ByteBase64);

        writer.WriteLine($"Keys: {hashes.Length}, Check: {md5Base64}");

        var di = CommonUtils.QualifyDestination(source, destination);
        var diSource = new DirectoryInfo(source);
        var items = diSource.EnumerateMedia(Av.Models.MediaTypes.Video, recurse: recurse, take: maxFiles);
        var total = items.Count();
        var done = 0;

        writer.WriteLine($"Collation: Start - Files: {total}");
        foreach (var file in items)
        {
            CollateInternal(file.FullName, key, di.FullName, itemCount, columns, itemHeight);
            writer.WriteLine($"Done: {++done * 100.0 / total:N2}%");
        }

        writer.WriteLine("Collation: End");
    }

    private static string CollateInternal(
        string source,
        byte[] key,
        string destination,
        int itemCount,
        int columns,
        int itemHeight)
    {
        var fi = new FileInfo(source);
        using var capper = CommonUtils.GetCapper(source, key);
        var opts = new CollationOptions { Columns = columns, ItemSize = new(0, itemHeight) };
        var memStr = capper.Collate(opts, itemCount);
        var fileName = $"{fi.FullName}_collation_x{itemCount}.jpg";

        // Re-encrypt; using the original key
        if (fi.IsSecure())
        {
            var saltHex = new AesGcmEncryptor()
                .Encrypt(memStr, memStr, key)
                .Encode(Codec.ByteHex)
                .ToLower(CultureInfo.InvariantCulture);
            fileName = fi.Name[..12] + "." + saltHex + ".jpg";
        }

        var path = Path.Combine(destination, fileName);
        File.WriteAllBytes(path, memStr.ToArray());
        return path;
    }
}