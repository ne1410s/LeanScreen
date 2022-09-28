using System;
using Av.Abstractions.Shared;

namespace Av.Abstractions.Rendering
{
    /// <summary>
    /// Obtains image stills from a video.
    /// </summary>
    public interface IRenderingService
    {
        TimeSpan Duration { get; }

        Dimensions2D FrameSize { get; }

        long TotalFrames { get; }

        /// <summary>
        /// Renders a frame at the position specified.
        /// </summary>
        /// <param name="position">The position.</param>
        /// <returns>The rendered frame.</returns>
        RenderedFrame RenderAt(TimeSpan position);
    }
}
