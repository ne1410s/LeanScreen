// <copyright file="FfmpegFormatConverter.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace LeanScreen.Rendering.Ffmpeg.Conversion;

using System;
using System.IO;
using CryptoStream.Streams;
using FFmpeg.AutoGen;
using LeanScreen.Abstractions.Conversion;
using LeanScreen.Rendering.Ffmpeg.IO;

/// <inheritdoc/>
public class FfmpegFormatConverter_002_Str2File : IFormatConverter
{
    /// <inheritdoc/>
    public unsafe Stream Remux(BlockStream source, string targetExt)
    {
        var out_filename = $@"C:\temp\~vids\out\111_strwip{targetExt}";

        FfmpegUtils.SetBinariesPath();
        FfmpegUtils.SetupLogging();

        AVFormatContext* ptrFormatCtx = null;
        AVPacket* ptrPacket = null;
        AVFormatContext* ptrOutputFmtCtx = null;
        AVOutputFormat* ptrOutputFormat = null;
        int* stream_mapping = null;

        try
        {
            using var uStream = new FfmpegUStream(source);
            avio_alloc_context_read_packet readFn = uStream.ReadUnsafe;
            avio_alloc_context_seek seekFn = uStream.SeekUnsafe;
            var bufLen = uStream.BufferLength;
            var ptrBuffer = (byte*)ffmpeg.av_malloc((ulong)bufLen);
            ptrFormatCtx = ffmpeg.avformat_alloc_context();
            ptrFormatCtx->pb = ffmpeg.avio_alloc_context(ptrBuffer, bufLen, 0, null, readFn, null, seekFn);

            // Process
            ptrPacket = ffmpeg.av_packet_alloc();
            ffmpeg.avformat_open_input(&ptrFormatCtx, string.Empty, null, null).avThrowIfError();
            ////ffmpeg.av_format_inject_global_side_data(ptrFormatCtx);
            ffmpeg.avformat_find_stream_info(ptrFormatCtx, null).avThrowIfError();
            ffmpeg.av_dump_format(ptrFormatCtx, 0, string.Empty, 0);

            ////AVCodec* ptrCodec = null;
            ////var ptrCodecCtx = ffmpeg.avcodec_alloc_context3(ptrCodec);
            ////AVCodec* codec = null;
            ////ffmpeg.avcodec_parameters_to_context(
            ////    ptrCodecCtx, ptrFormatCtx->streams[this.StreamIndex]->codecpar)
            ////        .avThrowIfError();
            ////ffmpeg.avcodec_open2(ptrCodecCtx, codec, null).avThrowIfError();

            // Output: file
            var streamIndex = 0;
            var stream_mapping_size = (int)ptrFormatCtx->nb_streams;
            stream_mapping = (int*)ffmpeg.av_calloc((ulong)stream_mapping_size, 4);
            ffmpeg.avformat_alloc_output_context2(&ptrOutputFmtCtx, null, null, out_filename).avThrowIfError();
            ptrOutputFormat = ptrOutputFmtCtx->oformat;

            // For each input stream
            for (var i = 0; i < stream_mapping_size; i++)
            {
                AVStream* out_stream;
                var in_stream = ptrFormatCtx->streams[i];
                var in_codecpar = in_stream->codecpar;

                if (in_codecpar->codec_type != AVMediaType.AVMEDIA_TYPE_AUDIO &&
                    in_codecpar->codec_type != AVMediaType.AVMEDIA_TYPE_VIDEO &&
                    in_codecpar->codec_type != AVMediaType.AVMEDIA_TYPE_SUBTITLE)
                {
                    stream_mapping[i] = -1;
                    continue;
                }

                stream_mapping[i] = streamIndex++;

                out_stream = ffmpeg.avformat_new_stream(ptrOutputFmtCtx, null);
                ffmpeg.avcodec_parameters_copy(out_stream->codecpar, in_codecpar).avThrowIfError();
                out_stream->codecpar->codec_tag = 0;
            }

            ffmpeg.av_dump_format(ptrOutputFmtCtx, 0, out_filename, 1);

            if ((ptrOutputFormat->flags & ffmpeg.AVFMT_NOFILE) == 0)
            {
                ffmpeg.avio_open(&ptrOutputFmtCtx->pb, out_filename, ffmpeg.AVIO_FLAG_WRITE).avThrowIfError();
            }

            // Write!
            ffmpeg.avformat_write_header(ptrOutputFmtCtx, null).avThrowIfError();
            while (true)
            {
                AVStream* in_stream;
                AVStream* out_stream;

                var read = ffmpeg.av_read_frame(ptrFormatCtx, ptrPacket);
                if (read == ffmpeg.AVERROR_EOF)
                {
                    break;
                }

                read.avThrowIfError();

                in_stream = ptrFormatCtx->streams[ptrPacket->stream_index];
                if (ptrPacket->stream_index >= stream_mapping_size || stream_mapping[ptrPacket->stream_index] < 0)
                {
                    ffmpeg.av_packet_unref(ptrPacket);
                    continue;
                }

                ptrPacket->stream_index = stream_mapping[ptrPacket->stream_index];
                out_stream = ptrOutputFmtCtx->streams[ptrPacket->stream_index];

                ffmpeg.av_packet_rescale_ts(ptrPacket, in_stream->time_base, out_stream->time_base);
                ptrPacket->pos = -1;

                ffmpeg.av_interleaved_write_frame(ptrOutputFmtCtx, ptrPacket).avThrowIfError();
            }

            ffmpeg.av_write_trailer(ptrOutputFmtCtx).avThrowIfError();

            // TODO: Stream!!
            return null!;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
            throw;
        }
        finally
        {
            ffmpeg.av_packet_free(&ptrPacket);
            ffmpeg.avformat_close_input(&ptrFormatCtx);

            if (ptrOutputFmtCtx != null && (ptrOutputFormat->flags & ffmpeg.AVFMT_NOFILE) == 0)
            {
                ffmpeg.avio_closep(&ptrOutputFmtCtx->pb);
            }

            ffmpeg.avformat_free_context(ptrOutputFmtCtx);
            ffmpeg.av_freep(&stream_mapping);
        }
    }
}
