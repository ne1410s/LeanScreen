// <copyright file="FfmpegUStream.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace LeanScreen.Rendering.Ffmpeg.IO;

using System;
using System.IO;
using FFmpeg.AutoGen;
using LeanScreen.Rendering.Ffmpeg.Decoding;

/// <summary>
/// Wraps a generic stream, exposing the interface to the internal workings of
/// ffmpeg. This is an unmanaged instance.
/// </summary>
/// <param name="input">The input stream.</param>
/// <param name="bufferLength">The buffer length.</param>
/// <param name="byteArrayCopier">A byte array copier.</param>
public sealed unsafe class FfmpegUStream(
    Stream input, int bufferLength = 32768, IByteArrayCopier? byteArrayCopier = null) : IUStream
{
    private const int SeekSize = ffmpeg.AVSEEK_SIZE;
    private static readonly int EOF = ffmpeg.AVERROR_EOF;

    private readonly object readLock = new();
    private readonly IByteArrayCopier byteArrayCopier = byteArrayCopier ?? new ByteArrayCopier();
    private readonly byte[] buffer = new byte[bufferLength];

    /// <inheritdoc/>
    public int BufferLength => bufferLength;

    /// <inheritdoc/>
    public int ReadUnsafe(void* opaque, byte* buffer, int count) =>
        this.TryManipulateStream(EOF, () =>
        {
            var read = input.Read(this.buffer, 0, bufferLength);
            if (read > 0)
            {
                this.byteArrayCopier.Copy(this.buffer, (IntPtr)buffer, read);
            }

            return read == 0 ? EOF : read;
        });

    /// <inheritdoc/>
    public long SeekUnsafe(void* opaque, long offset, int whence) =>
        this.TryManipulateStream(EOF, () =>
        {
            return whence == SeekSize
                ? input.Length
                : input.Seek(offset, SeekOrigin.Begin);
        });

    /// <inheritdoc/>
    public int WriteUnsafe(void* opaque, byte* buffer, int count) =>
        this.TryManipulateStream(EOF, () =>
        {
            this.byteArrayCopier.Copy((IntPtr)buffer, this.buffer, count);
            input.Write(this.buffer, 0, count);
            return count;
        });

    /// <inheritdoc/>
    public void Dispose()
    {
        input?.Dispose();
    }

    private T TryManipulateStream<T>(T fallback, Func<T> operation)
    {
        lock (this.readLock)
        {
            try
            {
                return operation();
            }
            catch
            {
                return fallback;
            }
        }
    }
}
