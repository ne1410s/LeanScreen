// <copyright file="SnapshotModule.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace AvCtl;

using System;
using System.Globalization;
using Av;
using Av.Abstractions.Shared;
using Av.Models;
using Comanche.Attributes;
using Comanche.Services;
using Crypt.Encoding;
using Crypt.Hashing;
using Crypt.IO;
using Crypt.Keying;
using Crypt.Transform;

/// <summary>
/// Snapshot module.
/// </summary>
[Module("snap")]
public static class SnapshotModule
{
    /// <summary>
    /// Generates frames from a video source.
    /// </summary>
    /// <param name="writer">Output writer.</param>
    /// <param name="source">The source file.</param>
    /// <param name="keySource">The key source directory.</param>
    /// <param name="keyRegex">The key source regular expression.</param>
    /// <param name="destination">The output folder.</param>
    /// <param name="itemCount">The total number of items.</param>
    /// <param name="itemHeight">The item height. If not specified, the natural dimensions are used.</param>
    /// <returns>The output path.</returns>
    [Alias("evenly")]
    public static string SnapEvenly(
        [Hidden] IOutputWriter writer,
        [Alias("s")]string source,
        [Alias("ks")] string? keySource = null,
        [Alias("kr")] string? keyRegex = null,
        [Alias("d")]string? destination = null,
        [Alias("t")]int itemCount = 24,
        [Alias("ih")] int? itemHeight = null)
    {
        _ = writer ?? throw new ArgumentNullException(nameof(writer));
        var blendedInput = writer.CaptureStrings().Blend();
        var hashes = CommonUtils.GetHashes(keySource, keyRegex);
        var key = new DefaultKeyDeriver().DeriveKey(blendedInput, hashes);
        var md5Base64 = key.Hash(HashType.Md5).Encode(Codec.ByteBase64);

        writer.WriteLine($"Keys: {hashes.Length}, Check: {md5Base64}");
        var di = CommonUtils.QualifyDestination(source, destination);
        var thumbSize = itemHeight.HasValue ? new Size2D(0, itemHeight.Value) : (Size2D?)null;
        using var capper = CommonUtils.GetCapper(source, key, thumbSize);
        var times = capper.Media.Duration.DistributeEvenly(itemCount);

        for (var i = 0; i < times.Length; i++)
        {
            var frame = capper.Snap(times[i]);
            var itemNo = (i + 1L).FormatToUpperBound(itemCount);
            var frameNo = frame.FrameNumber.FormatToUpperBound(capper.Media.TotalFrames);
            var targetName = $"n{itemNo}_f{frameNo}.jpg";
            SaveFrame(frame, di, targetName, source, key);
        }

        return di.FullName;
    }

    /// <summary>
    /// Generates a single frame from a video source.
    /// </summary>
    /// <param name="writer">Output writer.</param>
    /// <param name="source">The source file.</param>
    /// <param name="keySource">The key source directory.</param>
    /// <param name="keyRegex">The key source regular expression.</param>
    /// <param name="destination">The output folder.</param>
    /// <param name="relative">The relative position, from 0 - 1.</param>
    /// <param name="itemHeight">The item height. If not specified, the natural dimensions are used.</param>
    /// <returns>The output path.</returns>
    [Alias("frame")]
    public static string SnapSingleFrame(
        [Hidden] IOutputWriter writer,
        [Alias("s")] string source,
        [Alias("ks")] string? keySource = null,
        [Alias("kr")] string? keyRegex = null,
        [Alias("d")] string? destination = null,
        [Alias("r")] double relative = .3,
        [Alias("ih")] int? itemHeight = null)
    {
        _ = writer ?? throw new ArgumentNullException(nameof(writer));
        var blendedInput = writer.CaptureStrings().Blend();
        var hashes = CommonUtils.GetHashes(keySource, keyRegex);
        var key = new DefaultKeyDeriver().DeriveKey(blendedInput, hashes);
        var md5Base64 = key.Hash(HashType.Md5).Encode(Codec.ByteBase64);

        writer.WriteLine($"Keys: {hashes.Length}, Check: {md5Base64}");
        var di = CommonUtils.QualifyDestination(source, destination);
        var thumbSize = itemHeight.HasValue ? new Size2D(0, itemHeight.Value) : (Size2D?)null;
        using var capper = CommonUtils.GetCapper(source, key, thumbSize);
        using var frame = capper.Snap(relative);
        var frameNo = frame.FrameNumber.FormatToUpperBound(capper.Media.TotalFrames);
        var targetName = $"p{relative}_f{frameNo}.jpg";
        return SaveFrame(frame, di, targetName, source, key);
    }

    private static string SaveFrame(
        CapturedFrame frame,
        DirectoryInfo targetFolder,
        string targetName,
        string sourcePath,
        byte[] key)
    {
        var fi = new FileInfo(sourcePath);
        var nameToUse = targetName;
        if (fi.IsSecure())
        {
            var saltHex = new AesGcmEncryptor()
                .Encrypt(frame.Image, frame.Image, key)
                .Encode(Codec.ByteHex)
                .ToLower(CultureInfo.InvariantCulture);
            nameToUse = fi.Name[..12] + "." + saltHex + ".jpg";
        }

        var path = Path.Combine(targetFolder.FullName, nameToUse);
        File.WriteAllBytes(path, frame.Image.ToArray());
        return path;
    }
}
