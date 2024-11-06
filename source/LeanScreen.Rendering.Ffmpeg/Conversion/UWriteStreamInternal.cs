// <copyright file="UWriteStreamInternal.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace LeanScreen.Rendering.Ffmpeg.Conversion;

using System;
using System.Collections.Generic;
using System.IO;
using CryptoStream.Streams;
using FFmpeg.AutoGen;
using LeanScreen.Rendering.Ffmpeg.Decoding;

/// <summary>
/// Wraps <see cref="BlockStream"/>, exposing the interface to the
/// internal workings of ffmpeg. This is an unmanaged instance.
/// </summary>
public unsafe sealed class UWriteStreamInternal : IUWriteStream
{
    private const int SeekSize = ffmpeg.AVSEEK_SIZE;
    private static readonly int EOF = ffmpeg.AVERROR_EOF;

    private readonly object readLock = new();
    private readonly BlockStream target;
    private readonly IByteArrayCopier byteArrayCopier;
    private readonly byte[] writeBuffer;

    /// <summary>
    /// Initializes a new instance of the <see cref="UWriteStreamInternal"/> class.
    /// </summary>
    /// <param name="target">The target stream.</param>
    /// <param name="byteArrayCopier">A byte array copier.</param>
    public UWriteStreamInternal(BlockStream target, IByteArrayCopier? byteArrayCopier = null)
    {
        this.target = target;
        this.byteArrayCopier = byteArrayCopier ?? new ByteArrayCopier();
        this.writeBuffer = new byte[this.BufferLength];
    }

    /// <inheritdoc/>
    public int BufferLength => this.target.BufferLength;

    /// <summary>
    /// Gets the write events.
    /// </summary>
    public ICollection<WriteEvent> Writes { get; } = [];

    /// <summary>
    /// Gets the seek events.
    /// </summary>
    public ICollection<SeekEvent> Seeks { get; } = [];

    /// <inheritdoc/>
    public int WriteUnsafe(void* opaque, byte* buffer, int bufferLength) =>
        this.TryManipulateStream(EOF, () =>
        {
            this.byteArrayCopier.Copy((IntPtr)buffer, this.writeBuffer, bufferLength);
            var span = this.writeBuffer.AsSpan(0, bufferLength).ToArray();
            var ogLength = this.target.Length;
            var ogPosition = this.target.Position;
            this.target.Write(span, 0, span.Length);
            var isDirty = (this.target.Length - ogLength) < span.Length;

            this.Writes.Add(new() { At = ogPosition, Length = bufferLength, Dirty = isDirty });

            return span.Length;
        });

    /// <inheritdoc/>
    public long SeekUnsafe(void* opaque, long offset, int whence) =>
        this.TryManipulateStream(EOF, () =>
        {
            this.Seeks.Add(new() { From = this.target.Position, To = offset });

            return whence == SeekSize
                ? this.target.Length
                : this.target.Seek(offset, SeekOrigin.Begin);
        });

    /// <inheritdoc/>
    public void Dispose()
    {
        this.target?.Dispose();
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

/// <summary>
/// Seek.
/// </summary>
public record SeekEvent
{
    /// <summary>
    /// Gets or sets the from.
    /// </summary>
    public long From { get; set; }

    /// <summary>
    /// Gets or sets the to.
    /// </summary>
    public long To { get; set; }
}

/// <summary>
/// Write.
/// </summary>
public record WriteEvent
{
    /// <summary>
    /// Gets or sets the at.
    /// </summary>
    public long At { get; set; }

    /// <summary>
    /// Gets or sets the length.
    /// </summary>
    public long Length { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether a dirty write.
    /// </summary>
    public bool Dirty { get; set; }
}