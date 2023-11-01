// <copyright file="SnapshotModule.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace AvCtl;

using System;
using Av;
using Comanche.Attributes;
using Comanche.Services;
using Crypt.Keying;

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
    /// <returns>The output path.</returns>
    [Alias("evenly")]
    public static string SnapEvenly(
        [Hidden] IOutputWriter writer,
        [Alias("s")]string source,
        [Alias("ks")] string? keySource = null,
        [Alias("kr")] string? keyRegex = null,
        [Alias("d")]string? destination = null,
        [Alias("t")]int itemCount = 24)
    {
        _ = writer ?? throw new ArgumentNullException(nameof(writer));
        var blendedInput = writer.CaptureStrings().Blend();
        var hashes = CommonUtils.GetHashes(keySource, keyRegex);
        var key = new DefaultKeyDeriver().DeriveKey(blendedInput, hashes);

        var di = CommonUtils.QualifyDestination(source, destination);
        using var capper = CommonUtils.GetCapper(source, key);
        var times = capper.Media.Duration.DistributeEvenly(itemCount);

        for (var i = 0; i < times.Length; i++)
        {
            var frame = capper.Snap(times[i]);
            var itemNo = (i + 1L).FormatToUpperBound(itemCount);
            var frameNo = frame.FrameNumber.FormatToUpperBound(capper.Media.TotalFrames);
            var path = Path.Combine(di.FullName, $"n{itemNo}_f{frameNo}.jpg");
            File.WriteAllBytes(path, frame.Image.ToArray());
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
    /// <returns>The output path.</returns>
    [Alias("single")]
    public static string SnapSingle(
        [Hidden] IOutputWriter writer,
        [Alias("s")] string source,
        [Alias("ks")] string? keySource = null,
        [Alias("kr")] string? keyRegex = null,
        [Alias("d")] string? destination = null,
        [Alias("r")] double relative = .3)
    {
        _ = writer ?? throw new ArgumentNullException(nameof(writer));
        var blendedInput = writer.CaptureStrings().Blend();
        var hashes = CommonUtils.GetHashes(keySource, keyRegex);
        var key = new DefaultKeyDeriver().DeriveKey(blendedInput, hashes);

        var di = CommonUtils.QualifyDestination(source, destination);
        using var capper = CommonUtils.GetCapper(source, key);
        using var frame = capper.Snap(relative);
        var frameNo = frame.FrameNumber.FormatToUpperBound(capper.Media.TotalFrames);
        var path = Path.Combine(di.FullName, $"p{relative}_f{frameNo}.jpg");
        File.WriteAllBytes(path, frame.Image.ToArray());
        return di.FullName;
    }
}
