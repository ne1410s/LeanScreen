using System;
using Av.Models;

namespace Av.Services
{
    /// <summary>
    /// Obtains image stills from a video.
    /// </summary>
    public interface IVideoFrameRenderingService
    {
        /// <summary>
        /// Loads a new rendering session.
        /// </summary>
        /// <param name="videoInput">The video input.</param>
        /// <param name="itemSize">Desired frame dimensions.</param>
        RenderingSessionInfo Load(string videoInput, Dimensions2D? itemSize);

        /// <summary>
        /// Renders a frame at the position specified.
        /// </summary>
        /// <param name="position">The position.</param>
        /// <returns>The rendered frame.</returns>
        RenderedFrame RenderAt(TimeSpan position);
    }
}
