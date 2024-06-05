// <copyright file="IUStream.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace LeanScreen.Rendering.Ffmpeg.Decoding;

using System;

/// <summary>
/// Exposes the managed streams to the internal workings of ffmpeg.
/// </summary>
internal unsafe interface IUStream : IDisposable
{
    /// <summary>
    /// Gets the buffer length.
    /// </summary>
    int BufferLength { get; }

    /// <summary>
    /// Gets a value indicating whether the stream is seekable.
    /// </summary>
    bool CanSeek { get; }

    /// <summary>
    /// Reads from the underlying stream and writes up to
    /// <paramref name="bufferLength"/> bytes to the
    /// <paramref name="buffer"/>. Returns the number of bytes that
    /// were written.
    /// </summary>
    /// <param name="opaque">An FFmpeg provided opaque reference.</param>
    /// <param name="buffer">The target buffer.</param>
    /// <param name="bufferLength">The target buffer length.</param>
    /// <returns>The number of bytes that have been read.</returns>
    int ReadUnsafe(void* opaque, byte* buffer, int bufferLength);

    /// <summary>
    /// Seeks to the specified offset. The offset can be in byte position or
    /// in time units. This is specified by the whence parameter which is
    /// one of the AVSEEK prefixed constants.
    /// </summary>
    /// <param name="opaque">The opaque.</param>
    /// <param name="offset">The offset.</param>
    /// <param name="whence">The whence.</param>
    /// <returns>The position read; in bytes or time scale.</returns>
    long SeekUnsafe(void* opaque, long offset, int whence);
}
