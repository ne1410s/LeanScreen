// <copyright file="UStreamInternal.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace Av.Rendering.Ffmpeg.Decoding;

using System;
using CryptoStream.Streams;
using FFmpeg.AutoGen;

/// <summary>
/// Wraps <see cref="ISimpleReadStream"/>, exposing the interface to the
/// internal workings of ffmpeg. This is an unmanaged instance.
/// </summary>
public unsafe sealed class UStreamInternal : IUStream
{
    private const int SeekSize = ffmpeg.AVSEEK_SIZE;
    private static readonly int EOF = ffmpeg.AVERROR_EOF;

    private readonly object readLock = new();
    private readonly ISimpleReadStream source;
    private readonly IByteArrayCopier byteArrayCopier;

    /// <summary>
    /// Initializes a new instance of the <see cref="UStreamInternal"/> class.
    /// </summary>
    /// <param name="source">The input stream.</param>
    /// <param name="byteArrayCopier">A byte array copier.</param>
    public UStreamInternal(ISimpleReadStream source, IByteArrayCopier? byteArrayCopier = null)
    {
        this.source = source;
        this.byteArrayCopier = byteArrayCopier ?? new ByteArrayCopier();
    }

    /// <inheritdoc/>
    public int BufferLength => this.source.BufferLength;

    /// <inheritdoc/>
    public bool CanSeek => this.source.CanSeek;

    /// <inheritdoc/>
    public int ReadUnsafe(void* opaque, byte* buffer, int bufferLength) =>
        this.TryManipulateStream(EOF, () =>
        {
            var read = this.source.Read();
            if (read.Length > 0)
            {
                this.byteArrayCopier.Copy(read, (IntPtr)buffer, read.Length);
            }

            return read.Length;
        });

    /// <inheritdoc/>
    public long SeekUnsafe(void* opaque, long offset, int whence) =>
        this.TryManipulateStream(EOF, () => whence == SeekSize
            ? this.source.Length
            : this.source.Seek(offset));

    /// <inheritdoc/>
    public void Dispose()
    {
        this.source?.Dispose();
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
