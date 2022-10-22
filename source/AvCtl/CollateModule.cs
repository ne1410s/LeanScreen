// <copyright file="CollateModule.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace AvCtl;

using System.IO;
using Av.Abstractions.Imaging;
using Av.Abstractions.Rendering;
using Av.Imaging.SixLabors;
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
    /// <param name="columns">The number of columns to use.</param>
    /// <param name="keyCsv">Key (if source is encrypted).</param>
    /// <returns>The output path.</returns>
    [Alias("evenly")]
    public static string CollateEvenly(
        [Alias("s")] string source,
        [Alias("d")] string? destination = null,
        [Alias("t")] int itemCount = 24,
        [Alias("c")] int columns = 4,
        [Alias("k")] string? keyCsv = null)
    {
        var di = CommonUtils.QualifyDestination(source, destination);
        var sourceName = new FileInfo(source).Name;
        var path = Path.Combine(di.FullName, $"{sourceName}_collation_x{itemCount}.jpg");
        var snapper = CommonUtils.GetSnapper(source, keyCsv, out _);
        var frameList = new List<RenderedFrame>();
        snapper.Generate((f, _) => frameList.Add(f), itemCount);

        var opts = new CollationOptions { Columns = columns };
        var memStr = new SixLaborsCollatingService().Collate(frameList, opts);
        File.WriteAllBytes(path, memStr.ToArray());
        return path;
    }
}
