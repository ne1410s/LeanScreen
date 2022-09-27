using System;
using Av.Abstractions.Shared;

namespace Av.Abstractions.Rendering
{
    /// <summary>
    /// A rendered frame.
    /// </summary>
    public class RenderedFrame
    {
        /// <summary>
        /// The rendered bytes, in RGB 24-byte format.
        /// </summary>
        public byte[] Rgb24Bytes { get; set; }

        /// <summary>
        /// The frame dimensions.
        /// </summary>
        public Dimensions2D Dimensions { get; set; }

        /// <summary>
        /// The chronological frame number.
        /// </summary>
        public int FrameNumber { get; set; }

        /// <summary>
        /// The presentation time of the frame.
        /// </summary>
        public TimeSpan Position { get; set; }
    }
}
