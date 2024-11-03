// <copyright file="IUWriteStream.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace LeanScreen.Rendering.Ffmpeg.Conversion;

using System;

/// <summary>
/// Exposes the managed streams to the internal workings of ffmpeg.
/// </summary>
internal unsafe interface IUWriteStream : IDisposable
{
    /// <summary>
    /// Gets the buffer length.
    /// </summary>
    int BufferLength { get; }

    /// <summary>
    /// Writes to the underlying stream.
    /// </summary>
    /// <param name="opaque">An FFmpeg provided opaque reference.</param>
    /// <param name="buffer">The source buffer.</param>
    /// <param name="bufferLength">The source buffer length.</param>
    /// <returns>The number of bytes that have been written.</returns>
    int WriteUnsafe(void* opaque, byte* buffer, int bufferLength);

    /// <summary>
    /// Seeks on the underlying stream.
    /// </summary>
    /// <param name="opaque">A FFmpeg provided opaque reference.</param>
    /// <param name="offset">The offset.</param>
    /// <param name="whence">The whence.</param>
    /// <returns>The position.</returns>
    long SeekUnsafe(void* opaque, long offset, int whence);
}
