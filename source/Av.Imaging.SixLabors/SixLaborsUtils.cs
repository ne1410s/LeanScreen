﻿// <copyright file="SixLaborsUtils.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace Av.Imaging.SixLabors
{
    using Av.Abstractions.Shared;
    using global::SixLabors.ImageSharp;
    using global::SixLabors.ImageSharp.Processing;

    /// <summary>
    /// Six Labors utilities.
    /// </summary>
    public static class SixLaborsUtils
    {
        /// <summary>
        /// Resizes the image to a given maximum height.
        /// </summary>
        /// <param name="image">The image.</param>
        /// <param name="targetSize">The target size.</param>
        public static void Resize(this Image image, Size2D targetSize)
        {
            image.Mutate(x => x.Resize(new ResizeOptions
            {
                Mode = ResizeMode.Max,
                Size = new Size(targetSize.Width, targetSize.Height),
            }));
        }
    }
}
