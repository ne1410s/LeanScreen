// <copyright file="Size2D.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace Av.Common;

using System;

/// <summary>
/// 2D size.
/// </summary>
public struct Size2D
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Size2D"/> struct.
    /// </summary>
    /// <param name="width">The width.</param>
    /// <param name="height">The height.</param>
    public Size2D(int width, int height)
    {
        this.Width = width;
        this.Height = height;
    }

    /// <summary>
    /// Gets or sets the width.
    /// </summary>
    public int Width { get; set; }

    /// <summary>
    /// Gets or sets the height.
    /// </summary>
    public int Height { get; set; }

    /// <summary>
    /// Obtains a new size, scaled in accordance to the target dimensions,
    /// optionally preserving the aspect ratio (whether either non-zero
    /// height or width are supplied).
    /// </summary>
    /// <param name="target">The target dimensions, containing a new height,
    /// a new width, or indeed both (which forces the aspect ratio).</param>
    /// <returns>The new size.</returns>
    /// <exception cref="ArgumentException">Invalid argument.</exception>
    public Size2D ResizeTo(Size2D target)
    {
        if (this.Width <= 0 || this.Height <= 0)
        {
            throw new ArgumentException("Source dimensions invalid.");
        }

        if ((target.Width == 0 && target.Height == 0) || target.Width < 0 || target.Height < 0)
        {
            throw new ArgumentException("Target dimensions invalid.", nameof(target));
        }

        var aspectRatio = (double)this.Width / this.Height;
        return new Size2D
        {
            Width = target.Width > 0 ? target.Width : (int)Math.Round(target.Height * aspectRatio),
            Height = target.Height > 0 ? target.Height : (int)Math.Round(target.Width / aspectRatio),
        };
    }
}
