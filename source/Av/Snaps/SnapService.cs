// <copyright file="SnapService.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace Av.Snaps;

using System;
using System.IO;
using System.Linq;
using Av.Imaging;
using Av.Rendering;
using Av.Services;

/// <inheritdoc cref="ISnapService"/>
public class SnapService(IRenderingSessionFactory rendererFactory, IImagingService imager) : ISnapService
{
    /// <inheritdoc/>
    public MediaInfo GetInfo(
        Stream stream, byte[] salt, byte[] key)
    {
        using var renderer = rendererFactory.Create(stream, salt, key, null);
        return renderer.Media;
    }

    /// <inheritdoc/>
    public MemoryStream Snap(
        Stream stream, byte[] salt, byte[] key, double position = 0.4, int? height = 300)
    {
        using var renderer = rendererFactory.Create(stream, salt, key, height);
        var absolutePosition = position * renderer.Media.Duration.TotalSeconds;
        var frame = renderer.RenderAt(TimeSpan.FromSeconds(absolutePosition));
        return imager.Encode(frame.Rgb24Bytes.ToArray(), frame.Dimensions);
    }

    /// <inheritdoc/>
    public MemoryStream Collate(
        Stream stream, byte[] salt, byte[] key, int total = 24, int columns = 4, int? height = 300)
    {
        using var renderer = rendererFactory.Create(stream, salt, key, height);
        var times = renderer.Media.Duration.DistributeEvenly(total);
        var frames = times.Select(renderer.RenderAt);
        var opts = new CollationOptions { Columns = columns, ItemSize = renderer.ThumbSize };
        return imager.Collate(frames, opts);
    }
}
