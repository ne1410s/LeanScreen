// <copyright file="PhysicalFfmpegDecoding.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace Av.Rendering.Ffmpeg.Decoding
{
    /// <summary>
    /// Ffmpeg decoding session for physical sources.
    /// </summary>
    public sealed unsafe class PhysicalFfmpegDecoding : FfmpegDecodingSessionBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PhysicalFfmpegDecoding"/> class.
        /// </summary>
        /// <param name="url">The url to the physical media.</param>
        public PhysicalFfmpegDecoding(string url)
            : base(url)
        {
            this.OpenInputContext();
        }
    }
}
