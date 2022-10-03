using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Av.Abstractions.Shared;
using FFmpeg.AutoGen;

namespace Av.Rendering.Ffmpeg.Decoding
{
    /// <inheritdoc cref="IFfmpegDecodingSession"/>
    public abstract unsafe class FfmpegDecodingSessionBase : IFfmpegDecodingSession
    {
        private readonly string url;
        private readonly AVCodec* ptrCodec;

        /// <summary>
        /// Initialises a new <see cref="FfmpegDecodingSessionBase"/>.
        /// </summary>
        /// <param name="url">The url (for physical media).</param>
        protected FfmpegDecodingSessionBase(string url)
        {
            FfmpegUtils.SetupBinaries();
            FfmpegUtils.SetupLogging();

            this.url = url;
            ptrCodec = null;
            PtrCodecContext = ffmpeg.avcodec_alloc_context3(ptrCodec);
            PtrFormatContext = ffmpeg.avformat_alloc_context();
            PtrReceivedFrame = ffmpeg.av_frame_alloc();
            PtrPacket = ffmpeg.av_packet_alloc();
            PtrFrame = ffmpeg.av_frame_alloc();
        }

        /// <inheritdoc/>
        public string CodecName { get; private set; }

        /// <inheritdoc/>
        public Dimensions2D Dimensions { get; private set; }

        /// <inheritdoc/>
        public TimeSpan Duration { get; private set; }

        /// <inheritdoc/>
        public AVPixelFormat PixelFormat { get; private set; }

        /// <inheritdoc/>
        public AVRational TimeBase { get; private set; }

        /// <inheritdoc/>
        public long TotalFrames { get; private set; }

        /// <summary>
        /// A pointer to the codec context.
        /// </summary>
        protected AVCodecContext* PtrCodecContext { get; private set; }

        /// <summary>
        /// A pointer to the input format context.
        /// </summary>
        protected AVFormatContext* PtrFormatContext { get; private set; }

        /// <summary>
        /// A pointer to the frame.
        /// </summary>
        protected AVFrame* PtrFrame { get; private set; }

        /// <summary>
        /// A pointer to the packet.
        /// </summary>
        protected AVPacket* PtrPacket { get; private set; }

        /// <summary>
        /// A pointer to the received frame.
        /// </summary>
        protected AVFrame* PtrReceivedFrame { get; private set; }

        /// <summary>
        /// The stream index.
        /// </summary>
        protected int StreamIndex { get; private set; }

        /// <summary>
        /// Opens the input format context.
        /// </summary>
        protected void OpenInputContext()
        {
            var pFormatContext = PtrFormatContext;
            //pFormatContext->seek2any = 1;
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

        /// <inheritdoc/>
        public virtual void Seek(TimeSpan position)
        {
            //var inferredFrame = TotalFrames * (position.TotalSeconds / Duration.TotalSeconds);
            //ffmpeg.av_seek_frame(PtrFormatContext, StreamIndex, (int)inferredFrame, ffmpeg.AVSEEK_FLAG_FRAME)
            //    .ThrowExceptionIfError();

            var ts = position.ToLong(TimeBase);
            ffmpeg.avformat_seek_file(PtrFormatContext, StreamIndex, long.MinValue, ts, ts, 0)
                .ThrowExceptionIfError();

            //ffmpeg.avcodec_flush_buffers(PtrCodecContext);
        }

        /// <inheritdoc/>
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

        /// <inheritdoc/>
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
            {
                frame = *PtrFrame;
            }

            return true;
        }

        /// <inheritdoc/>
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
