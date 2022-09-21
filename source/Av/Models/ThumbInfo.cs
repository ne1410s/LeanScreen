using System;

namespace Av.Models
{
    /// <summary>
    /// Thumbnail information.
    /// </summary>
    public class ThumbInfo
    {
        /// <summary>
        /// Gets or sets the width.
        /// </summary>
        public int Width { get; set; }

        /// <summary>
        /// Gets or sets the height.
        /// </summary>
        public int Height { get; set; }

        /// <summary>
        /// Gets or sets the frame number.
        /// </summary>
        public long FrameNumber { get; set; }

        /// <summary>
        /// Gets or sets the start time.
        /// </summary>
        public TimeSpan StartTime { get; set; }
    }
}
