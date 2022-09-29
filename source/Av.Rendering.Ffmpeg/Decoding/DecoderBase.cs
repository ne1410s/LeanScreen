using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Av.Abstractions.Shared;
using FFmpeg.AutoGen;

namespace Av.Rendering.Ffmpeg.Decoding
{
    internal abstract unsafe class DecoderBase : IDecoder
    {
        private readonly string url;
        private readonly AVCodec* ptrCodec;

        protected DecoderBase(string url)
        {
            this.url = url;
            ptrCodec = null;
            PtrCodecContext = ffmpeg.avcodec_alloc_context3(ptrCodec);
            PtrFormatContext = ffmpeg.avformat_alloc_context();
            PtrReceivedFrame = ffmpeg.av_frame_alloc();
            PtrPacket = ffmpeg.av_packet_alloc();
            PtrFrame = ffmpeg.av_frame_alloc();
        }

        public string CodecName { get; private set; }
        
        public Dimensions2D Dimensions { get; private set; }
        
        public TimeSpan Duration { get; private set; }
        
        public AVPixelFormat PixelFormat { get; private set; }
        
        public AVRational TimeBase { get; private set; }
        
        public long TotalFrames { get; private set; }
        
        protected AVCodecContext* PtrCodecContext { get; private set; }
        
        protected AVFormatContext* PtrFormatContext { get; private set; }
        
        protected AVFrame* PtrFrame { get; private set; }
        
        protected AVPacket* PtrPacket { get; private set; }
        
        protected AVFrame* PtrReceivedFrame { get; private set; }
        
        protected int StreamIndex { get; private set; }

        protected void OpenInputContext()
        {
            var pFormatContext = PtrFormatContext;
            pFormatContext->seek2any = 1;
            ffmpeg.avformat_open_input(&pFormatContext, url, null, null).ThrowExceptionIfError();
            ffmpeg.av_format_inject_global_side_data(PtrFormatContext);
            ffmpeg.avformat_find_stream_info(PtrFormatContext, null).ThrowExceptionIfError();
            AVCodec* codec = null;

            StreamIndex = ffmpeg
                .av_find_best_stream(PtrFormatContext, AVMediaType.AVMEDIA_TYPE_VIDEO, -1, -1, &codec, 0)
                .ThrowExceptionIfError();
            ffmpeg.avcodec_parameters_to_context(PtrCodecContext, PtrFormatContext->streams[StreamIndex]->codecpar)
                .ThrowExceptionIfError();
            ffmpeg.avcodec_open2(PtrCodecContext, codec, null).ThrowExceptionIfError();

            TimeBase = PtrFormatContext->streams[StreamIndex]->time_base;
            Duration = PtrFormatContext->duration.ToTimeSpan(ffmpeg.AV_TIME_BASE);
            CodecName = ffmpeg.avcodec_get_name(codec->id);
            Dimensions = new Dimensions2D { Width = PtrCodecContext->width, Height = PtrCodecContext->height };
            PixelFormat = PtrCodecContext->pix_fmt;
            TotalFrames = PtrFormatContext->streams[StreamIndex]->nb_frames;
        }

        public virtual void Seek(TimeSpan position)
        {
            //ffmpeg.avcodec_flush_buffers(_pCodecContext);

            var ts = position.ToLong(TimeBase);
            var res = ffmpeg.avformat_seek_file(PtrFormatContext, StreamIndex, long.MinValue, ts, ts, 0);

            //var res = ffmpeg.av_seek_frame(_pFormatContext, _streamIndex, ts, ffmpeg.AVSEEK_FLAG_BACKWARD);
            //var res = ffmpeg.av_seek_frame(_pFormatContext, _streamIndex, ts, ffmpeg.AVSEEK_FLAG_ANY);
        }

        public virtual void Dispose()
        {
            var pFrame = PtrFrame;
            ffmpeg.av_frame_free(&pFrame);

            var pPacket = PtrPacket;
            ffmpeg.av_packet_free(&pPacket);

            ffmpeg.avcodec_close(PtrCodecContext);
            var pFormatContext = PtrFormatContext;
            ffmpeg.avformat_close_input(&pFormatContext);

            PtrCodecContext = null;
            PtrFormatContext = null;
        }

        public virtual bool TryDecodeNextFrame(out AVFrame frame)
        {
            ffmpeg.av_frame_unref(PtrFrame);
            ffmpeg.av_frame_unref(PtrReceivedFrame);
            int error;

            do
            {
                try
                {
                    do
                    {
                        ffmpeg.av_packet_unref(PtrPacket);
                        error = ffmpeg.av_read_frame(PtrFormatContext, PtrPacket);

                        if (error == ffmpeg.AVERROR_EOF)
                        {
                            frame = *PtrFrame;
                            return false;
                        }

                        error.ThrowExceptionIfError();
                    } while (PtrPacket->stream_index != StreamIndex);

                    ffmpeg.avcodec_send_packet(PtrCodecContext, PtrPacket).ThrowExceptionIfError();
                }
                finally
                {
                    ffmpeg.av_packet_unref(PtrPacket);
                }

                error = ffmpeg.avcodec_receive_frame(PtrCodecContext, PtrFrame);
            } while (error == ffmpeg.AVERROR(ffmpeg.EAGAIN));

            error.ThrowExceptionIfError();

            if (PtrCodecContext->hw_device_ctx != null)
            {
                ffmpeg.av_hwframe_transfer_data(PtrReceivedFrame, PtrFrame, 0).ThrowExceptionIfError();
                frame = *PtrReceivedFrame;
            }
            else
                frame = *PtrFrame;

            return true;
        }

        public IReadOnlyDictionary<string, string> GetContextInfo()
        {
            AVDictionaryEntry* tag = null;
            var result = new Dictionary<string, string>();

            while ((tag = ffmpeg.av_dict_get(PtrFormatContext->metadata, "", tag, ffmpeg.AV_DICT_IGNORE_SUFFIX)) != null)
            {
                var key = Marshal.PtrToStringAnsi((IntPtr)tag->key);
                var value = Marshal.PtrToStringAnsi((IntPtr)tag->value);
                result.Add(key, value);
            }

            return result;
        }
    }
}
