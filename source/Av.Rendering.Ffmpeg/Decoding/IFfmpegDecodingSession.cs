// <copyright file="IFfmpegDecodingSession.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace Av.Rendering.Ffmpeg.Decoding
{
    using System;
    using System.Collections.Generic;
    using Av.Abstractions.Shared;
    using FFmpeg.AutoGen;

    /// <summary>
    /// Ffmpeg decoding session.
    /// </summary>
    public interface IFfmpegDecodingSession : IDisposable
    {
        /// <summary>
        /// Gets the codec name.
        /// </summary>
        string CodecName { get; }

        /// <summary>
        /// Gets the dimensions.
        /// </summary>
        Size2D Dimensions { get; }

        /// <summary>
        /// Gets the duration.
        /// </summary>
        TimeSpan Duration { get; }

        /// <summary>
        /// Gets the pixel format.
        /// </summary>
        AVPixelFormat PixelFormat { get; }

        /// <summary>
        /// Gets the time base.
        /// </summary>
        AVRational TimeBase { get; }

        /// <summary>
        /// Gets the total frames.
        /// </summary>
        long TotalFrames { get; }

        /// <summary>
        /// Gets the average frame rate.
        /// </summary>
        double FrameRate { get; }

        /// <summary>
        /// Seeks to the specified position, returning the nearest key frame.
        /// </summary>
        /// <param name="position">The position.</param>
        /// <returns>The nearest key frame.</returns>
        AVFrame Seek(TimeSpan position);

        /// <summary>
        /// Gets context information.
        /// </summary>
        /// <returns>A dictionary of context data.</returns>
        IReadOnlyDictionary<string, string> GetContextInfo();
    }
}
