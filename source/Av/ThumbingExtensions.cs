// <copyright file="ThumbingExtensions.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace Av
{
    using System;
    using System.Linq;
    using Av.Abstractions.Shared;

    /// <summary>
    /// Extensions to assist with the production of thumbnails.
    /// </summary>
    public static class ThumbingExtensions
    {
        /// <summary>
        /// Gets an evenly-distributed sequence of times.
        /// </summary>
        /// <param name="duration">The total duration.</param>
        /// <param name="count">The number of items to distribute.</param>
        /// <returns>A sequence of evenly-distributed times.</returns>
        public static TimeSpan[] DistributeEvenly(this TimeSpan duration, int count)
        {
            var deltaMs = duration.TotalMilliseconds / (Math.Max(2, count) - 1);
            return Enumerable.Range(0, count)
                .Select(n => TimeSpan.FromMilliseconds(deltaMs * n))
                .ToArray();
        }

        /// <summary>
        /// Obtains a new size, scaled in accordance to the target dimensions,
        /// optionally preserving the aspect ratio (whether either non-zero
        /// height or width are supplied).
        /// </summary>
        /// <param name="source">The original dimensions.</param>
        /// <param name="target">The target dimensions, containing a new height,
        /// a new width, or indeed both (which forces the aspect ratio).</param>
        /// <returns>The new size.</returns>
        /// <exception cref="ArgumentException">Invalid argument.</exception>
        public static Dimensions2D ResizeTo(this Dimensions2D source, Dimensions2D target)
        {
            if (source.Width <= 0 || source.Height <= 0)
            {
                throw new ArgumentException("Dimensions invalid.", nameof(source));
            }

            if ((target.Width == 0 && target.Height == 0) || target.Width < 0 || target.Height < 0)
            {
                throw new ArgumentException("Dimensions invalid.", nameof(target));
            }

            var aspectRatio = (double)source.Width / source.Height;
            return new Dimensions2D
            {
                Width = target.Width > 0 ? target.Width : (int)Math.Round(target.Height * aspectRatio),
                Height = target.Height > 0 ? target.Height : (int)Math.Round(target.Width / aspectRatio),
            };
        }
    }
}
