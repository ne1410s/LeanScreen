// <copyright file="IRenderingService.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace Av.Abstractions.Rendering
{
    using System;
    using Av.Abstractions.Shared;

    /// <summary>
    /// Obtains image stills from a video.
    /// </summary>
    public interface IRenderingService
    {
        /// <summary>
        /// Gets the duration.
        /// </summary>
        TimeSpan Duration { get; }

        /// <summary>
        /// Gets the target frame size.
        /// </summary>
        Dimensions2D FrameSize { get; }

        /// <summary>
        /// Gets the total number of frames.
        /// </summary>
        long TotalFrames { get; }

        /// <summary>
        /// Renders a frame at the position specified.
        /// </summary>
        /// <param name="position">The position.</param>
        /// <returns>The rendered frame.</returns>
        RenderedFrame RenderAt(TimeSpan position);
    }
}
