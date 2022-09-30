﻿namespace Av.Rendering.Ffmpeg
{
    /// <summary>
    /// Raw frame data.
    /// </summary>
    internal class RawFrame
    {
        /// <summary>
        /// The rendered bytes, in RGB 24-byte format.
        /// </summary>
        public byte[] Rgb24Bytes { get; set; }

        /// <summary>
        /// The presentation time of the frame.
        /// </summary>
        public long PresentationTime { get; set; }
    }
}
