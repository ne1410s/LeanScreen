using System;
using Av.Abstractions.Shared;

namespace Av.Abstractions.Rendering
{
    /// <summary>
    /// Obtains image stills from a video.
    /// </summary>
    public interface IRenderingService
    {
        /// <summary>
        /// The duration.
        /// </summary>
        TimeSpan Duration { get; }

        /// <summary>
        /// The target frame size.
        /// </summary>
        Dimensions2D FrameSize { get; }

        /// <summary>
        /// The total number of frames.
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
