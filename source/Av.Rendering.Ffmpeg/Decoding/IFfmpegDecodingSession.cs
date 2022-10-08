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
        Dimensions2D Dimensions { get; }

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
