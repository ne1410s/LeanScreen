// <copyright file="CollateModule.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace AvCtl;

using System.Globalization;
using System.IO;
using Av;
using Av.Abstractions.Imaging;
using Av.Abstractions.Rendering;
using Av.Imaging.SixLabors;
using Comanche.Attributes;
using Comanche.Services;
using Crypt.Encoding;
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
    /// <param name="source">The source file.</param>
    /// <param name="keySource">The key source directory.</param>
    /// <param name="keyRegex">The key source regular expression.</param>
    /// <param name="destination">The output folder.</param>
    /// <param name="itemCount">The total number of items.</param>
    /// <param name="columns">The number of columns to use.</param>
    /// <param name="itemHeight">The item height to set.</param>
    /// <param name="writer">Output writer.</param>
    /// <returns>The output path.</returns>
    [Alias("evenly")]
    public static string CollateEvenly(
        [Alias("s")] string source,
        [Alias("ks")] string? keySource = null,
        [Alias("kr")] string? keyRegex = null,
        [Alias("d")] string? destination = null,
        [Alias("t")] int itemCount = 24,
        [Alias("c")] int columns = 4,
        [Alias("h")] int itemHeight = 300,
        IOutputWriter? writer = null)
    {
        writer ??= new ConsoleWriter();
        var blendedInput = writer.CaptureStrings().Blend();
        var hashes = CommonUtils.GetHashes(keySource, keyRegex);
        var key = new DefaultKeyDeriver().DeriveKey(blendedInput, hashes);

        var di = CommonUtils.QualifyDestination(source, destination);
        var fi = new FileInfo(source);
        var snapper = CommonUtils.GetSnapper(source, key, out _);
        var frameList = new List<RenderedFrame>();
        snapper.Generate((f, _) => frameList.Add(f), itemCount);
        var opts = new CollationOptions { Columns = columns, ItemSize = new(0, itemHeight) };
        var memStr = new SixLaborsCollatingService().Collate(frameList, opts);
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

        var path = Path.Combine(di.FullName, fileName);
        File.WriteAllBytes(path, memStr.ToArray());
        return path;
    }

    /// <summary>
    /// Collates all video files in source.
    /// </summary>
    /// <param name="source">The source directory.</param>
    /// <param name="keySource">The key source directory.</param>
    /// <param name="keyRegex">The key source regular expression.</param>
    /// <param name="destination">The output folder.</param>
    /// <param name="itemCount">The total number of items.</param>
    /// <param name="columns">The number of columns to use.</param>
    /// <param name="itemHeight">The item height to set.</param>
    /// <param name="writer">Output writer.</param>
    [Alias("bulk")]
    public static void CollateManyEvenly(
        [Alias("s")] string source,
        [Alias("ks")] string? keySource = null,
        [Alias("kr")] string? keyRegex = null,
        [Alias("d")] string? destination = null,
        [Alias("t")] int itemCount = 24,
        [Alias("c")] int columns = 4,
        [Alias("h")] int itemHeight = 300,
        IOutputWriter? writer = null)
    {
        var diSource = new DirectoryInfo(source);
        var items = diSource.EnumerateMedia(Av.Models.MediaTypes.Video);
        writer ??= new ConsoleWriter();
        var total = items.Count();
        var done = 0;

        writer.WriteLine($"Collation: Start - Files: {total}");
        foreach (var file in items)
        {
            CollateEvenly(file.FullName, keySource, keyRegex, destination, itemCount, columns, itemHeight, writer);
            writer.WriteLine($"Done: {++done * 100.0 / total:N2}%");
        }

        writer.WriteLine("Collation: End");
    }
}