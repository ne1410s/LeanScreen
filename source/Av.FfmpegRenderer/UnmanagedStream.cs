using System;
using System.Runtime.InteropServices;
using Crypt.Streams;
using FFmpeg.AutoGen;

namespace Av.Renderer.Ffmpeg
{
    /// <summary>
    /// Wraps <see cref="ISimpleReadStream"/>, exposing the interface to the
    /// internal workings of ffmpeg.
    /// </summary>
    internal unsafe class UnmanagedStream : IDisposable
    {
        private const int SeekSize = ffmpeg.AVSEEK_SIZE;
        private static readonly int EOF = ffmpeg.AVERROR_EOF;

        private readonly object readLock = new object();
        private readonly ISimpleReadStream source;

        /// <summary>
        /// Initialises a new instance of the <see cref="UnmanagedStream"/>.
        /// </summary>
        /// <param name="source">The input stream.</param>
        public UnmanagedStream(ISimpleReadStream source)
        {
            this.source = source;
        }

        /// <summary>
        /// Gets the buffer length.
        /// </summary>
        public int BufferLength => source.BufferLength;

        /// <summary>
        /// Gets a value indicating whether the stream is seekable.
        /// </summary>
        public bool CanSeek => source.CanSeek;

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
        public int ReadUnsafe(void* opaque, byte* buffer, int bufferLength) =>
            TryManipulateStream(EOF, () =>
            {
                var read = source.Read();
                if (read.Length > 0)
                {
                    Marshal.Copy(read, 0, (IntPtr)buffer, read.Length);
                }

                return read.Length;
            });

        /// <summary>
        /// Seeks to the specified offset. The offset can be in byte position or
        /// in time units. This is specified by the whence parameter which is
        /// one of the AVSEEK prefixed constants.
        /// </summary>
        /// <param name="opaque">The opaque.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="whence">The whence.</param>
        /// <returns>The position read; in bytes or time scale.</returns>
        public long SeekUnsafe(void* opaque, long offset, int whence) =>
            TryManipulateStream(EOF, () => whence == SeekSize
                ? source.Length
                : source.Seek(offset));

        /// <inheritdoc/>
        public void Dispose()
        {
            source?.Dispose();
        }

        private T TryManipulateStream<T>(T fallback, Func<T> operation)
        {
            lock (readLock)
            {
                try { return operation(); }
                catch { return fallback; }
            }
        }
    }
}
