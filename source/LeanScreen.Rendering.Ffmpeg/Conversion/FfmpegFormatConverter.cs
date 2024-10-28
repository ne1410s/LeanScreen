// <copyright file="FfmpegFormatConverter.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace LeanScreen.Rendering.Ffmpeg.Conversion;

using System.IO;
using CryptoStream.Streams;
using FFmpeg.AutoGen;
using LeanScreen.Abstractions.Conversion;
using LeanScreen.Rendering.Ffmpeg.Decoding;

/// <inheritdoc/>
public class FfmpegFormatConverter : IFormatConverter
{
    /// <inheritdoc/>
    public unsafe Stream Remux(ISimpleReadStream source, MediaInfo sourceInfo)
    {
        FfmpegUtils.SetBinariesPath();
        FfmpegUtils.SetupLogging();

        AVCodec* ptrCodec = null;
        var ptrCodecCtx = ffmpeg.avcodec_alloc_context3(ptrCodec);
        var ptrFormatCtx = ffmpeg.avformat_alloc_context();

        using var uStream = new UStreamInternal(source);
        avio_alloc_context_read_packet readFn = uStream.ReadUnsafe;
        avio_alloc_context_seek seekFn = uStream.SeekUnsafe;
        var bufLen = uStream.BufferLength;

        var ptrBuffer = (byte*)ffmpeg.av_malloc((ulong)bufLen);
        var streamIc = ffmpeg.avio_alloc_context(
            ptrBuffer, bufLen, 0, null, readFn, null, seekFn);

        ptrFormatCtx->pb = streamIc;

        // Process
        AVPacket* ptrPacket = null;
        ptrPacket = ffmpeg.av_packet_alloc();
        ffmpeg.avformat_open_input(&ptrFormatCtx, string.Empty, null, null).avThrowIfError();
        ////ffmpeg.av_format_inject_global_side_data(ptrFormatCtx);
        ffmpeg.avformat_find_stream_info(ptrFormatCtx, null).avThrowIfError();
        ffmpeg.av_dump_format(ptrFormatCtx, 0, string.Empty, 0);

        // Output
        var ptrOutputCtx = ffmpeg.avformat_alloc_context();

        //AVCodec* codec = null;
        //ffmpeg.avcodec_parameters_to_context(
        //    ptrCodecCtx, ptrFormatCtx->streams[this.StreamIndex]->codecpar)
        //        .avThrowIfError();
        //ffmpeg.avcodec_open2(ptrCodecCtx, codec, null).avThrowIfError();

    }
}
