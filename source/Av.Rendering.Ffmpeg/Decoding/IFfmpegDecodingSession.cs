using System;
using System.Collections.Generic;
using Av.Abstractions.Shared;
using FFmpeg.AutoGen;

namespace Av.Rendering.Ffmpeg.Decoding
{
    /// <summary>
    /// Ffmpeg decoding session.
    /// </summary>
    internal interface IFfmpegDecodingSession : IDisposable
    {
        /// <summary>
        /// The codec name.
        /// </summary>
        string CodecName { get; }

        /// <summary>
        /// The dimensions.
        /// </summary>
        Dimensions2D Dimensions { get; }

        /// <summary>
        /// The duration.
        /// </summary>
        TimeSpan Duration { get; }

        /// <summary>
        /// The pixel format.
        /// </summary>
        AVPixelFormat PixelFormat { get; }

        /// <summary>
        /// The time base.
        /// </summary>
        AVRational TimeBase { get; }

        /// <summary>
        /// The total frames.
        /// </summary>
        long TotalFrames { get; }

        /// <summary>
        /// Seeks to the specified position.
        /// </summary>
        /// <param name="position">The position.</param>
        void Seek(TimeSpan position);

        /// <summary>
        /// Obtains the next frame.
        /// </summary>
        /// <param name="frame">The frame.</param>
        /// <returns>Whether successful.</returns>
        bool TryDecodeNextFrame(out AVFrame frame);

        /// <summary>
        /// Gets context information.
        /// </summary>
        /// <returns>A dictionary of context data.</returns>
        IReadOnlyDictionary<string, string> GetContextInfo();
    }
}
