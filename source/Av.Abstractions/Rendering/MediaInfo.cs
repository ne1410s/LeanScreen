// <copyright file="MediaInfo.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace Av.Abstractions.Rendering
{
    using System;
    using Av.Abstractions.Shared;

    /// <summary>
    /// Original media info.
    /// </summary>
    public class MediaInfo
    {
        /// <summary>
        /// Initialises a new instance of the <see cref="MediaInfo"/> class.
        /// </summary>
        /// <param name="duration">The duration.</param>
        /// <param name="dimensions">The dimensions.</param>
        /// <param name="totalFrames">The total number of frames.</param>
        /// <param name="frameRate">The average frame rate.</param>
        public MediaInfo(TimeSpan duration, Dimensions2D dimensions, long totalFrames, double frameRate)
        {
            this.Duration = duration;
            this.Dimensions = dimensions;
            this.TotalFrames = totalFrames;
            this.FrameRate = frameRate;
        }

        /// <summary>
        /// Gets the duration.
        /// </summary>
        public TimeSpan Duration { get; }

        /// <summary>
        /// Gets the frame size.
        /// </summary>
        public Dimensions2D Dimensions { get; }

        /// <summary>
        /// Gets the total number of frames.
        /// </summary>
        public long TotalFrames { get; }

        /// <summary>
        /// Gets the average frame rate.
        /// </summary>
        public double FrameRate { get; }
    }
}
