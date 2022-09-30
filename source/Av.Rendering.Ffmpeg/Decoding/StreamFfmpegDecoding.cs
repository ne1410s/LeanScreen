using Crypt.Streams;
using FFmpeg.AutoGen;

namespace Av.Rendering.Ffmpeg.Decoding
{
    /// <summary>
    /// Ffmpeg decoding session for stream sources.
    /// </summary>
    internal sealed unsafe class StreamFfmpegDecoding : FfmpegDecodingSessionBase
    {
        private readonly ISimpleReadStream readStream;
        private IUStream uStream;
        private avio_alloc_context_read_packet readFn;
        private avio_alloc_context_seek seekFn;
        private AVIOContext* streamIc;

        /// <summary>
        /// Initialises a new <see cref="StreamFfmpegDecoding"/>.
        /// </summary>
        /// <param name="stream">A stream.</param>
        public StreamFfmpegDecoding(ISimpleReadStream stream)
            : base(string.Empty)
        {
            readStream = stream;
            uStream = new UStream(stream);
            readFn = uStream.ReadUnsafe;
            seekFn = uStream.SeekUnsafe;
            var bufLen = uStream.BufferLength;
            var ptrBuffer = (byte*)ffmpeg.av_malloc((ulong)bufLen);
            streamIc = ffmpeg.avio_alloc_context(ptrBuffer, bufLen, 0, null, readFn, null, seekFn);
            streamIc->seekable = uStream.CanSeek ? 1 : 0;
            PtrFormatContext->pb = streamIc;

            OpenInputContext();
        }

        /// <inheritdoc/>
        public override void Dispose()
        {
            base.Dispose();

            ffmpeg.av_freep(&streamIc->buffer);
            var customInputContext = streamIc;
            ffmpeg.av_freep(&customInputContext);
            streamIc = null;
            readFn = null;
            seekFn = null;
            uStream?.Dispose();
            uStream = null;
            readStream?.Dispose();
        }
    }
}
