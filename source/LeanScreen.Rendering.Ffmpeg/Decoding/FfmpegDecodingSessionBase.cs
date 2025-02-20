﻿// <copyright file="FfmpegDecodingSessionBase.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace LeanScreen.Rendering.Ffmpeg.Decoding;

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using FFmpeg.AutoGen;
using LeanScreen.Common;

/// <inheritdoc cref="IFfmpegDecodingSession"/>
public abstract unsafe class FfmpegDecodingSessionBase : IFfmpegDecodingSession
{
    private const byte SeekThresholdMs = 150;

    /// <summary>
    /// Initializes a new instance of the <see cref="FfmpegDecodingSessionBase"/> class.
    /// </summary>
    /// <param name="url">The url (for physical media).</param>
    protected FfmpegDecodingSessionBase(string url)
    {
        FfmpegUtils.SetBinariesPath();
        FfmpegUtils.SetupLogging();

        this.Url = url;
        AVCodec* ptrCodec = null;
        this.PtrCodecContext = ffmpeg.avcodec_alloc_context3(ptrCodec);
        this.PtrFormatContext = ffmpeg.avformat_alloc_context();
        this.PtrReceivedFrame = ffmpeg.av_frame_alloc();
        this.PtrPacket = ffmpeg.av_packet_alloc();
        this.PtrFrame = ffmpeg.av_frame_alloc();
    }

    /// <summary>
    /// Gets the url.
    /// </summary>
    public string Url { get; }

    /// <inheritdoc/>
    public string? CodecName { get; private set; }

    /// <inheritdoc/>
    public Size2D Dimensions { get; private set; }

    /// <inheritdoc/>
    public TimeSpan Duration { get; private set; }

    /// <inheritdoc/>
    public AVPixelFormat PixelFormat { get; private set; }

    /// <inheritdoc/>
    public AVRational TimeBase { get; private set; }

    /// <inheritdoc/>
    public long TotalFrames { get; private set; }

    /// <inheritdoc/>
    public double FrameRate { get; private set; }

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
    public AVFrame Seek(TimeSpan position)
    {
        var ts = position.ToLong(this.TimeBase);
        _ = ffmpeg.avformat_seek_file(this.PtrFormatContext, this.StreamIndex, long.MinValue, ts, ts, 0);

        AVFrame retVal;
        double msAhead;

        var iter = 1;
        double previousMsAhead = double.MinValue;
        while (true)
        {
            iter++;
            var readOk = this.TryDecodeNextFrame(out retVal);
            var framePosition = ((double)retVal.best_effort_timestamp).ToTimeSpan(this.TimeBase);
            msAhead = (framePosition - position).TotalMilliseconds;

            // bail out if error, or bullseye
            if (!readOk || (int)msAhead == (int)previousMsAhead || position == TimeSpan.Zero
                || Math.Abs(msAhead) <= SeekThresholdMs)
            {
                break;
            }

            // rewind if we've overshot the mark
            if (msAhead > SeekThresholdMs)
            {
                previousMsAhead = msAhead;
                var seekPos = (position - TimeSpan.FromSeconds(1)).Clamp(this.Duration);
                var newTs = seekPos.ToLong(this.TimeBase);
                _ = ffmpeg.avformat_seek_file(this.PtrFormatContext, this.StreamIndex, long.MinValue, newTs, newTs, 0)
                    .avThrowIfError();
            }
            else
            {
                // keep reading 'em frames
                previousMsAhead = double.MinValue;
            }
        }

        return retVal;
    }

    /// <inheritdoc/>
    public virtual void Dispose()
    {
        var pFrame = this.PtrFrame;
        ffmpeg.av_frame_free(&pFrame);

        var pPacket = this.PtrPacket;
        ffmpeg.av_packet_free(&pPacket);

        var pCodecContext = this.PtrCodecContext;
        ffmpeg.avcodec_free_context(&pCodecContext);
        var pFormatContext = this.PtrFormatContext;
        ffmpeg.avformat_close_input(&pFormatContext);

        this.PtrCodecContext = null;
        this.PtrFormatContext = null;
        this.StreamIndex = -1;

        GC.SuppressFinalize(this);
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

        // NB: This has always been commented-out...
        ////pFormatContext->seek2any = 1;

        _ = ffmpeg.avformat_open_input(&pFormatContext, this.Url, null, null).avThrowIfError();
        ffmpeg.av_format_inject_global_side_data(this.PtrFormatContext);
        _ = ffmpeg.avformat_find_stream_info(this.PtrFormatContext, null).avThrowIfError();
        AVCodec* codec = null;

        this.StreamIndex = ffmpeg
            .av_find_best_stream(this.PtrFormatContext, AVMediaType.AVMEDIA_TYPE_VIDEO, -1, -1, &codec, 0)
            .avThrowIfError();
        _ = ffmpeg.avcodec_parameters_to_context(
            this.PtrCodecContext, PtrFormatContext->streams[this.StreamIndex]->codecpar)
                .avThrowIfError();
        _ = ffmpeg.avcodec_open2(this.PtrCodecContext, codec, null).avThrowIfError();

        var frameRate = PtrFormatContext->streams[this.StreamIndex]->avg_frame_rate;
        this.FrameRate = (double)frameRate.num / frameRate.den;
        this.TimeBase = PtrFormatContext->streams[this.StreamIndex]->time_base;
        this.Duration = ((double)PtrFormatContext->duration).ToTimeSpan(new());
        this.CodecName = codec->id.ToString();
        this.Dimensions = new Size2D { Width = PtrCodecContext->width, Height = PtrCodecContext->height };
        this.PixelFormat = PtrCodecContext->pix_fmt;
        this.TotalFrames = PtrFormatContext->streams[this.StreamIndex]->nb_frames;
        if (this.TotalFrames == 0)
        {
            this.TotalFrames = (long)Math.Round(this.Duration.TotalSeconds * this.FrameRate);
        }
    }

    /// <summary>
    /// Decodes the next frame.
    /// </summary>
    /// <param name="frame">The frame.</param>
    /// <returns>Whether successful.</returns>
    private bool TryDecodeNextFrame(out AVFrame frame)
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

                    _ = error.avThrowIfError();
                }
                while (PtrPacket->stream_index != this.StreamIndex);

                _ = ffmpeg.avcodec_send_packet(this.PtrCodecContext, this.PtrPacket).avThrowIfError();
            }
            finally
            {
                ffmpeg.av_packet_unref(this.PtrPacket);
            }

            error = ffmpeg.avcodec_receive_frame(this.PtrCodecContext, this.PtrFrame);
        }
        while (error == ffmpeg.AVERROR(ffmpeg.EAGAIN));

        _ = error.avThrowIfError();

        // Can we select a hw device automatically?
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
}
