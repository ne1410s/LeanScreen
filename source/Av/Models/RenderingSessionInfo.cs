using System;

namespace Av.Models
{
    /// <summary>
    /// Information for a rendering session.
    /// </summary>
    public class RenderingSessionInfo
    {
        /// <summary>
        /// The video total length.
        /// </summary>
        public TimeSpan Duration { get; set; }

        /// <summary>
        /// The video dimensions.
        /// </summary>
        public Dimensions2D Dimensions { get; set; }
    }
}
