using System;
using System.Runtime.InteropServices;
using Crypt.Streams;
using FFmpeg.AutoGen;

namespace Av.Rendering.Ffmpeg.Decoding
{
    /// <summary>
    /// Wraps <see cref="ISimpleReadStream"/>, exposing the interface to the
    /// internal workings of ffmpeg. This is an unmanaged instance.
    /// </summary>
    internal unsafe class UStream : IUStream
    {
        private const int SeekSize = ffmpeg.AVSEEK_SIZE;
        private static readonly int EOF = ffmpeg.AVERROR_EOF;

        private readonly object readLock = new object();
        private readonly ISimpleReadStream source;

        /// <summary>
        /// Initialises a new instance of the <see cref="UStream"/>.
        /// </summary>
        /// <param name="source">The input stream.</param>
        public UStream(ISimpleReadStream source)
        {
            this.source = source;
        }

        /// <inheritdoc/>
        public int BufferLength => source.BufferLength;

        /// <inheritdoc/>
        public bool CanSeek => source.CanSeek;

        /// <inheritdoc/>
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

        /// <inheritdoc/>
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
