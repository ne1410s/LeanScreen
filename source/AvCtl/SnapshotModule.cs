// <copyright file="SnapshotModule.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace AvCtl;

using Av;
using Av.Abstractions.Rendering;
using Av.Imaging.SixLabors;
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
    /// <param name="source">The source file.</param>
    /// <param name="keySource">The key source directory.</param>
    /// <param name="keyRegex">The key source regular expression.</param>
    /// <param name="destination">The output folder.</param>
    /// <param name="itemCount">The total number of items.</param>
    /// <param name="writer">Output writer.</param>
    /// <returns>The output path.</returns>
    [Alias("evenly")]
    public static string SnapEvenly(
        [Alias("s")]string source,
        [Alias("ks")] string? keySource = null,
        [Alias("kr")] string? keyRegex = null,
        [Alias("d")]string? destination = null,
        [Alias("t")]int itemCount = 24,
        IOutputWriter? writer = null)
    {
        writer ??= new ConsoleWriter();
        var blendedInput = writer.CaptureStrings().Blend();
        var hashes = CommonUtils.GetHashes(keySource, keyRegex);
        var key = new DefaultKeyDeriver().DeriveKey(blendedInput, hashes);

        var di = CommonUtils.QualifyDestination(source, destination);
        var snapper = CommonUtils.GetSnapper(source, key, out var renderer);
        var imager = new SixLaborsImagingService();
        var onFrameReceived = (RenderedFrame frame, int index) =>
        {
            var itemNo = (index + 1L).FormatToUpperBound(itemCount);
            var frameNo = frame.FrameNumber.FormatToUpperBound(renderer.Media.TotalFrames);
            var path = Path.Combine(di.FullName, $"n{itemNo}_f{frameNo}.jpg");
            using var memStr = imager.Encode(frame.Rgb24Bytes, frame.Dimensions);
            File.WriteAllBytes(path, memStr.ToArray());
        };

        snapper.Generate(onFrameReceived, itemCount);
        return di.FullName;
    }

    /// <summary>
    /// Generates a single frame from a video source.
    /// </summary>
    /// <param name="source">The source file.</param>
    /// <param name="keySource">The key source directory.</param>
    /// <param name="keyRegex">The key source regular expression.</param>
    /// <param name="destination">The output folder.</param>
    /// <param name="relative">The relative position, from 0 - 1.</param>
    /// <param name="writer">Output writer.</param>
    /// <returns>The output path.</returns>
    [Alias("single")]
    public static string SnapSingle(
        [Alias("s")] string source,
        [Alias("ks")] string? keySource = null,
        [Alias("kr")] string? keyRegex = null,
        [Alias("d")] string? destination = null,
        [Alias("r")] double relative = .3,
        IOutputWriter? writer = null)
    {
        writer ??= new ConsoleWriter();
        var blendedInput = writer.CaptureStrings().Blend();
        var hashes = CommonUtils.GetHashes(keySource, keyRegex);
        var key = new DefaultKeyDeriver().DeriveKey(blendedInput, hashes);

        var di = CommonUtils.QualifyDestination(source, destination);
        var snapper = CommonUtils.GetSnapper(source, key, out var renderer);
        var imager = new SixLaborsImagingService();
        var onFrameReceived = (RenderedFrame frame, int _) =>
        {
            var frameNo = frame.FrameNumber.FormatToUpperBound(renderer.Media.TotalFrames);
            var path = Path.Combine(di.FullName, $"p{relative}_f{frameNo}.jpg");
            using var memStr = imager.Encode(frame.Rgb24Bytes, frame.Dimensions);
            File.WriteAllBytes(path, memStr.ToArray());
        };

        snapper.Generate(onFrameReceived, renderer.Media.Duration * relative);
        return di.FullName;
    }
}
