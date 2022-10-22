// <copyright file="CollateModule.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace AvCtl;

using System.Globalization;
using System.IO;
using Av;
using Av.Abstractions.Rendering;
using Av.Imaging.SixLabors;
using Av.Rendering.Ffmpeg;
using Av.Services;
using Comanche;

/// <summary>
/// Collate module.
/// </summary>
[Alias("collate")]
public static class CollateModule
{
    /// <summary>
    /// Generates frames from a video source into a collated image.
    /// </summary>
    /// <param name="source">The source file.</param>
    /// <param name="destination">The output folder.</param>
    /// <param name="itemCount">The total number of items.</param>
    /// <param name="keyCsv">Key (if source is encrypted).</param>
    /// <returns>The output path.</returns>
    [Alias("evenly")]
    public static string CollateEvenly(
        [Alias("s")] string source,
        [Alias("d")] string? destination = null,
        [Alias("t")] int itemCount = 24,
        [Alias("k")] string? keyCsv = null)
    {
        var fi = new FileInfo(source);
        if (destination == null && fi.Exists)
        {
            destination = fi.DirectoryName;
        }

        var key = keyCsv?.Split(',').Select(b => byte.Parse(b, CultureInfo.InvariantCulture)).ToArray();
        IRenderingService renderer = new FfmpegRenderer(source, key);
        var di = new DirectoryInfo(destination ?? Directory.GetCurrentDirectory());
        var snapper = new ThumbnailGenerator(renderer);
        var collator = new SixLaborsCollatingService();
        var frameList = new List<RenderedFrame>();
        var onFrameReceived = (RenderedFrame frame, int _) => frameList.Add(frame);
        snapper.Generate(onFrameReceived, itemCount);
        var memStr = collator.Collate(frameList);
        var path = Path.Combine(di.FullName, $"collation_x{itemCount}.jpg");
        File.WriteAllBytes(path, memStr.ToArray());
        return path;
    }
}
