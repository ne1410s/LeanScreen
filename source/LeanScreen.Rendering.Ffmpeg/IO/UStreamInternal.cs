// <copyright file="UStreamInternal.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace LeanScreen.Rendering.Ffmpeg.IO;

using System;
using System.IO;
using CryptoStream.Streams;
using FFmpeg.AutoGen;
using LeanScreen.Rendering.Ffmpeg.Decoding;

/// <summary>
/// Wraps <see cref="IBlockStream"/>, exposing the interface to the
/// internal workings of ffmpeg. This is an unmanaged instance.
/// </summary>
/// <param name="blockStr">The input stream.</param>
/// <param name="byteArrayCopier">A byte array copier.</param>
public unsafe sealed class UStreamInternal(BlockStream blockStr, IByteArrayCopier? byteArrayCopier = null) : IUStream
{
    private const int SeekSize = ffmpeg.AVSEEK_SIZE;
    private static readonly int EOF = ffmpeg.AVERROR_EOF;

    private readonly object readLock = new();
    private readonly IByteArrayCopier byteArrayCopier = byteArrayCopier ?? new ByteArrayCopier();
    private readonly byte[] buffer = new byte[blockStr.BufferLength];
    private readonly MemoryStream headerWriteBuffer = new(blockStr.BufferLength);
    private readonly MemoryStream trailerWriteBuffer = new(blockStr.BufferLength);

    private long trailerStartPosition = long.MaxValue;
    private bool bufferTrailerMode;

    /// <inheritdoc/>
    public int BufferLength => blockStr.BufferLength;

    /// <summary>
    /// Gets or sets a value indicating whether to buffer write operation.
    /// </summary>
    public bool BufferTrailerMode
    {
        get => this.bufferTrailerMode;
        set
        {
            this.bufferTrailerMode = value;
            if (value)
            {
                this.trailerStartPosition = blockStr.Position;
            }
        }
    }

    /// <inheritdoc/>
    public int ReadUnsafe(void* opaque, byte* buffer, int bufferLength) =>
        this.TryManipulateStream(EOF, () =>
        {
            var read = blockStr.Read(this.buffer, 0, bufferLength);
            if (read > 0)
            {
                this.byteArrayCopier.Copy(this.buffer, (IntPtr)buffer, read);
            }

            return read == 0 ? EOF : read;
        });

    /// <inheritdoc/>
    public long SeekUnsafe(void* opaque, long offset, int whence) =>
        this.TryManipulateStream(EOF, () => whence == SeekSize
            ? blockStr.Length
            : blockStr.Seek(offset, SeekOrigin.Begin));

    /// <inheritdoc/>
    public int WriteUnsafe(void* opaque, byte* buffer, int bufferLength) =>
        this.TryManipulateStream(EOF, () =>
        {
            this.byteArrayCopier.Copy((IntPtr)buffer, this.buffer, bufferLength);

            var headerWrite = blockStr.Position < this.BufferLength;
            var dirtyWrite = blockStr.Position != blockStr.Length;
            if (headerWrite)
            {
                if (dirtyWrite)
                {
                    this.headerWriteBuffer.Seek(blockStr.Position, SeekOrigin.Begin);
                }

                var bit = this.buffer.AsSpan(0, bufferLength);
                this.headerWriteBuffer.Write(bit.ToArray(), 0, bufferLength);
            }

            if (dirtyWrite)
            {
                if (headerWrite && blockStr.Position + bufferLength > this.BufferLength)
                {
                    throw new InvalidOperationException("Cannot overwrite header beyond the first block.");
                }
                else if (!this.BufferTrailerMode)
                {
                    throw new InvalidOperationException("Cannot overwrite trailer as buffer mode is not enabled.");
                }
            }

            if (this.BufferTrailerMode && !headerWrite)
            {
                var trailerSeek = blockStr.Position - this.trailerStartPosition;
                this.trailerWriteBuffer.Seek(trailerSeek, SeekOrigin.Begin);
                this.trailerWriteBuffer.Write(this.buffer, 0, bufferLength);
            }

            blockStr.Write(this.buffer, 0, bufferLength);
            return bufferLength;
        });

    /// <summary>
    /// Finalises write.
    /// </summary>
    public void FinaliseWrite()
    {
        // send up whatever is left
        blockStr.FlushCache();

        // (re)-write header block
        blockStr.Seek(0, SeekOrigin.Begin);
        blockStr.Write(this.headerWriteBuffer.ToArray(), 0, this.BufferLength);
        this.headerWriteBuffer.SetLength(0);

        // write header
        blockStr.Seek(blockStr.Length, SeekOrigin.Begin);
        this.trailerWriteBuffer.CopyTo(blockStr, this.BufferLength);
        this.trailerWriteBuffer.SetLength(0);

        blockStr.FinaliseWrite();
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        blockStr?.Dispose();
    }

    private T TryManipulateStream<T>(T fallback, Func<T> operation)
    {
        lock (this.readLock)
        {
            try
            {
                return operation();
            }
            catch (Exception ex)
            {
                return fallback;
            }
        }
    }
}
