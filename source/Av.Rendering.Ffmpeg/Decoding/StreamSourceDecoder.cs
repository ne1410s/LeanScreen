using Crypt.Streams;
using FFmpeg.AutoGen;

namespace Av.Rendering.Ffmpeg.Decoding
{
    internal sealed unsafe class StreamSourceDecoder : DecoderBase
    {
        private UnmanagedStream uStream;
        private avio_alloc_context_read_packet readFn;
        private avio_alloc_context_seek seekFn;
        private AVIOContext* streamIc;

        public StreamSourceDecoder(ISimpleReadStream stream)
            : base(string.Empty)
        {
            // TODO: Compare with StreamSourceDecoder_OLD
            // As it would seem this produced better results than below...?

            uStream = new UnmanagedStream(stream);
            readFn = uStream.ReadUnsafe;
            seekFn = uStream.SeekUnsafe;
            var bufLen = uStream.BufferLength;
            var ptrBuffer = (byte*)ffmpeg.av_malloc((ulong)bufLen);
            streamIc = ffmpeg.avio_alloc_context(ptrBuffer, bufLen, 0, null, readFn, null, seekFn);
            streamIc->seekable = uStream.CanSeek ? 1 : 0;
            PtrFormatContext->pb = streamIc;

            OpenInputContext();
        }

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
        }
    }
}
