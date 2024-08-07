﻿// <copyright file="SixLaborsImagingService.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace LeanScreen.Imaging.SixLabors;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using global::SixLabors.ImageSharp;
using global::SixLabors.ImageSharp.Formats.Jpeg;
using global::SixLabors.ImageSharp.PixelFormats;
using global::SixLabors.ImageSharp.Processing;
using LeanScreen.Common;
using LeanScreen.Imaging;
using LeanScreen.Rendering;

/// <inheritdoc cref="IImagingService"/>
public class SixLaborsImagingService : IImagingService
{
    private const int CollationBorderSize = 1;
    private static readonly Argb32 White = new(255, 255, 255);
    private static readonly Argb32 Black = new(0, 0, 0);
    private static readonly Argb32 Transparent = new(0, 0, 0, 0);

    /// <inheritdoc/>
    public async Task<Size2D> GetSize(Stream stream)
    {
        var (image, _) = await Image.LoadWithFormatAsync(stream);
        return new(image.Width, image.Height);
    }

    /// <inheritdoc/>
    public MemoryStream Encode(byte[] rgb24Bytes, Size2D size)
    {
        var image = Image.LoadPixelData<Rgb24>(rgb24Bytes, size.Width, size.Height);
        var retVal = new MemoryStream();
        image.Save(retVal, new JpegEncoder());
        retVal.Position = 0;
        return retVal;
    }

    /// <inheritdoc/>
    public async Task<MemoryStream> ResizeImage(Stream stream, Size2D targetSize)
    {
        var (image, format) = await Image.LoadWithFormatAsync(stream);
        var retVal = new MemoryStream();
        image.Resize(targetSize);
        await image.SaveAsync(retVal, format);
        return retVal;
    }

    /// <inheritdoc/>
    public MemoryStream Collate(IEnumerable<RenderedFrame> frames, CollationOptions? opts = null)
    {
        opts ??= new CollationOptions();
        var frameList = frames?.ToList() ?? [];
        if (frameList.Count == 0)
        {
            throw new ArgumentException("No frames found.", nameof(frames));
        }

        var firstItemSize = frameList[0].Dimensions;
        var itemSize = opts.ItemSize == null ? firstItemSize : firstItemSize.ResizeTo(opts.ItemSize.Value);
        var map = opts.GetMap(itemSize, frameList.Count);
        var borderColour = opts.UseItemBorder ? Black : Transparent;
        using var canvas = new Image<Argb32>(map.CanvasSize.Width, map.CanvasSize.Height, White);
        using var border = new Image<Argb32>(
            itemSize.Width + (CollationBorderSize * 2),
            itemSize.Height + (CollationBorderSize * 2),
            borderColour);
        var iterIndex = 0;
        foreach (var frame in frameList)
        {
            var inSize = frame.Dimensions;
            var item = Image.LoadPixelData<Rgb24>(frame.Rgb24Bytes.ToArray(), inSize.Width, inSize.Height);
            var coords = map.Coordinates[iterIndex++];
            item.Resize(map.ItemSize);
            canvas.Mutate(o => o
                .DrawImage(border, new Point(coords.X - CollationBorderSize, coords.Y - CollationBorderSize), 1)
                .DrawImage(item, new Point(coords.X, coords.Y), 1));
            item.Dispose();
        }

        var retVal = new MemoryStream();
        canvas.Save(retVal, new JpegEncoder());
        retVal.Position = 0;
        return retVal;
    }
}