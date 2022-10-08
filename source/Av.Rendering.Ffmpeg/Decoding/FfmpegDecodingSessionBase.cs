// <copyright file="FfmpegDecodingSessionBase.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace Av.Rendering.Ffmpeg.Decoding
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using Av.Abstractions.Shared;
    using FFmpeg.AutoGen;

    /// <inheritdoc cref="IFfmpegDecodingSession"/>
    public abstract unsafe class FfmpegDecodingSessionBase : IFfmpegDecodingSession
    {
        private readonly string url;
        private readonly AVCodec* ptrCodec;

        /// <summary>
        /// Initialises a new instance of the <see cref="FfmpegDecodingSessionBase"/> class.
        /// </summary>
        /// <param name="url">The url (for physical media).</param>
        protected FfmpegDecodingSessionBase(string url)
        {
            FfmpegUtils.SetupBinaries();
            FfmpegUtils.SetupLogging();

            this.url = url;
            this.ptrCodec = null;
            this.PtrCodecContext = ffmpeg.avcodec_alloc_context3(this.ptrCodec);
            this.PtrFormatContext = ffmpeg.avformat_alloc_context();
            this.PtrReceivedFrame = ffmpeg.av_frame_alloc();
            this.PtrPacket = ffmpeg.av_packet_alloc();
            this.PtrFrame = ffmpeg.av_frame_alloc();
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
        /// Gets a pointer to the codec context.
        /// </summary>
        protected AVCodecContext* PtrCodecContext { get; private set; }

        /// <summary>
        /// Gets a pointer to the input format context.
        /// </summary>
        protected AVFormatContext* PtrFormatContext { get; private set; }

        /// <summary>
        /// Gets a pointer to the frame.
        /// </summary>
        protected AVFrame* PtrFrame { get; private set; }

        /// <summary>
        /// Gets a pointer to the packet.
        /// </summary>
        protected AVPacket* PtrPacket { get; private set; }

        /// <summary>
        /// Gets a pointer to the received frame.
        /// </summary>
        protected AVFrame* PtrReceivedFrame { get; private set; }

        /// <summary>
        /// Gets the stream index.
        /// </summary>
        protected int StreamIndex { get; private set; }

        /// <inheritdoc/>
        public virtual void Seek(TimeSpan position)
        {
            var ts = position.ToLong(this.TimeBase);
            ffmpeg.avformat_seek_file(this.PtrFormatContext, this.StreamIndex, long.MinValue, ts, ts, 0)
                .ThrowExceptionIfError();
        }

        /// <inheritdoc/>
        public virtual void Dispose()
        {
            var pFrame = this.PtrFrame;
            ffmpeg.av_frame_free(&pFrame);

            var pPacket = this.PtrPacket;
            ffmpeg.av_packet_free(&pPacket);

            ffmpeg.avcodec_close(this.PtrCodecContext);
            var pFormatContext = this.PtrFormatContext;
            ffmpeg.avformat_close_input(&pFormatContext);

            this.PtrCodecContext = null;
            this.PtrFormatContext = null;

            GC.SuppressFinalize(this);
        }

        /// <inheritdoc/>
        public virtual bool TryDecodeNextFrame(out AVFrame frame)
        {
            ffmpeg.av_frame_unref(this.PtrFrame);
            ffmpeg.av_frame_unref(this.PtrReceivedFrame);
            int error;

            do
            {
                try
                {
                    do
                    {
                        ffmpeg.av_packet_unref(this.PtrPacket);
                        error = ffmpeg.av_read_frame(this.PtrFormatContext, this.PtrPacket);

                        if (error == ffmpeg.AVERROR_EOF)
                        {
                            frame = *this.PtrFrame;
                            return false;
                        }

                        error.ThrowExceptionIfError();
                    }
                    while (PtrPacket->stream_index != this.StreamIndex);

                    ffmpeg.avcodec_send_packet(this.PtrCodecContext, this.PtrPacket).ThrowExceptionIfError();
                }
                finally
                {
                    ffmpeg.av_packet_unref(this.PtrPacket);
                }

                error = ffmpeg.avcodec_receive_frame(this.PtrCodecContext, this.PtrFrame);
            }
            while (error == ffmpeg.AVERROR(ffmpeg.EAGAIN));

            error.ThrowExceptionIfError();

            // TODO: Can we select a hw device automatically?
            // ... and does it improve frame capture??

            ////if (PtrCodecContext->hw_device_ctx != null)
            ////{
            ////    ffmpeg.av_hwframe_transfer_data(PtrReceivedFrame, PtrFrame, 0).ThrowExceptionIfError();
            ////    frame = *PtrReceivedFrame;
            ////}
            ////else
            ////{
            frame = *this.PtrFrame;
            ////}

            return true;
        }

        /// <inheritdoc/>
        public IReadOnlyDictionary<string, string> GetContextInfo()
        {
            AVDictionaryEntry* tag = null;
            var result = new Dictionary<string, string>();

            while ((tag = ffmpeg.av_dict_get(
                PtrFormatContext->metadata, string.Empty, tag, ffmpeg.AV_DICT_IGNORE_SUFFIX)) != null)
            {
                var key = Marshal.PtrToStringAnsi((IntPtr)tag->key);
                var value = Marshal.PtrToStringAnsi((IntPtr)tag->value);
                result.Add(key, value);
            }

            return result;
        }

        /// <summary>
        /// Opens the input format context.
        /// </summary>
        protected void OpenInputContext()
        {
            var pFormatContext = this.PtrFormatContext;
            ////pFormatContext->seek2any = 1;
            ffmpeg.avformat_open_input(&pFormatContext, this.url, null, null).ThrowExceptionIfError();
            ffmpeg.av_format_inject_global_side_data(this.PtrFormatContext);
            ffmpeg.avformat_find_stream_info(this.PtrFormatContext, null).ThrowExceptionIfError();
            AVCodec* codec = null;

            this.StreamIndex = ffmpeg
                .av_find_best_stream(this.PtrFormatContext, AVMediaType.AVMEDIA_TYPE_VIDEO, -1, -1, &codec, 0)
                .ThrowExceptionIfError();
            ffmpeg.avcodec_parameters_to_context(
                this.PtrCodecContext, PtrFormatContext->streams[this.StreamIndex]->codecpar)
                    .ThrowExceptionIfError();
            ffmpeg.avcodec_open2(this.PtrCodecContext, codec, null).ThrowExceptionIfError();

            this.TimeBase = PtrFormatContext->streams[this.StreamIndex]->time_base;
            this.Duration = PtrFormatContext->duration.ToTimeSpan(ffmpeg.AV_TIME_BASE);
            this.CodecName = ffmpeg.avcodec_get_name(codec->id);
            this.Dimensions = new Dimensions2D { Width = PtrCodecContext->width, Height = PtrCodecContext->height };
            this.PixelFormat = PtrCodecContext->pix_fmt;
            this.TotalFrames = PtrFormatContext->streams[this.StreamIndex]->nb_frames;
        }
    }
}
