using System;
using Crypt.Streams;
using FFmpeg.AutoGen;

namespace Av.Rendering.Ffmpeg.Decoding
{
    internal sealed unsafe class StreamSourceDecoder : DecoderBase
    {
        private UnmanagedStream unmanagedStream;
        private avio_alloc_context_read_packet customInputStreamRead;
        private avio_alloc_context_seek customInputStreamSeek;
        private AVIOContext* customInputStreamContext;

        public StreamSourceDecoder(ISimpleReadStream stream)
            : base(string.Empty)
        {
            unmanagedStream = new UnmanagedStream(stream);
            customInputStreamRead = unmanagedStream.ReadUnsafe;
            customInputStreamSeek = unmanagedStream.SeekUnsafe;
            var inputBuffer = (byte*)ffmpeg.av_malloc(
                (ulong)unmanagedStream.BufferLength);
            customInputStreamContext = ffmpeg.avio_alloc_context(
                inputBuffer,
                unmanagedStream.BufferLength,
                0,
                null,
                customInputStreamRead,
                null,
                customInputStreamSeek);
            customInputStreamContext->seekable = unmanagedStream.CanSeek ? 1 : 0;
            PtrFormatContext->pb = customInputStreamContext;
            PtrFormatContext->seek2any = 1;

            Initialise();
        }

        public override void Seek(TimeSpan position)
        {
            //ffmpeg.avcodec_flush_buffers(_pCodecContext);

            var ts = position.ToLong(TimeBase);
            var res = ffmpeg.avformat_seek_file(PtrFormatContext, StreamIndex, long.MinValue, ts, ts, 0);

            //var res = ffmpeg.av_seek_frame(_pFormatContext, _streamIndex, ts, ffmpeg.AVSEEK_FLAG_BACKWARD);
            //var res = ffmpeg.av_seek_frame(_pFormatContext, _streamIndex, ts, ffmpeg.AVSEEK_FLAG_ANY);
        }

        public override void Dispose()
        {
            base.Dispose();

            ffmpeg.av_freep(&customInputStreamContext->buffer);
            var customInputContext = customInputStreamContext;
            ffmpeg.av_freep(&customInputContext);
            customInputStreamContext = null;
            customInputStreamRead = null;
            customInputStreamSeek = null;
            unmanagedStream?.Dispose();
            unmanagedStream = null;
        }
    }
}
