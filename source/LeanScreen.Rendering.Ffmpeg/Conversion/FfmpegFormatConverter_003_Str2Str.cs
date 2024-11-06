// <copyright file="FfmpegFormatConverter_003_Str2Str.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace LeanScreen.Rendering.Ffmpeg.Conversion;

using System;
using System.IO;
using System.Linq;
using CryptoStream.Streams;
using FFmpeg.AutoGen;
using LeanScreen.Rendering.Ffmpeg.Decoding;

/// <summary>
/// Third iteration.
/// </summary>
public unsafe class FfmpegFormatConverter_003_Str2Str
{
    /// <summary>
    /// Remultiplexes a source stream to a target.
    /// </summary>
    /// <param name="source">The source stream.</param>
    /// <param name="target">The target stream.</param>
    /// <param name="ext">The target extension.</param>
    /// <param name="salt">The salt.</param>
    /// <param name="key">The key.</param>
    /// <returns>Suggested name.</returns>
    public string Remux(Stream source, Stream target, string ext, byte[] salt, byte[] key)
    {
        salt ??= [];
        var withCrypto = salt.Length > 0;
        using var readStream = withCrypto
            ? new GcmCryptoStream(source, salt, key)
            : new BlockStream(source);
        using var writeStream = withCrypto
            ? new GcmCryptoStream(target, salt, key, ext)
            : new BlockStream(target);

        var targetName = writeStream is GcmCryptoStream cs ? cs.Id : $"result{ext}";

        FfmpegUtils.SetBinariesPath();
        FfmpegUtils.SetupLogging();

        AVPacket* ptrPacket = null;
        AVFormatContext* ptrInputFmtCtx = null;
        AVFormatContext* ptrOutputFmtCtx = null;
        int* stream_mapping = null;

        try
        {
            using var uStream = new UStreamInternal(readStream);
            avio_alloc_context_read_packet readFn = uStream.ReadUnsafe;
            avio_alloc_context_seek seekFn = uStream.SeekUnsafe;
            var bufLen = uStream.BufferLength;
            var ptrBuffer = (byte*)ffmpeg.av_malloc((ulong)bufLen);
            ptrInputFmtCtx = ffmpeg.avformat_alloc_context();
            ptrInputFmtCtx->pb = ffmpeg.avio_alloc_context(ptrBuffer, bufLen, 0, null, readFn, null, seekFn);

            // Process
            ptrPacket = ffmpeg.av_packet_alloc();
            ffmpeg.avformat_open_input(&ptrInputFmtCtx, string.Empty, null, null).avThrowIfError();
            ffmpeg.avformat_find_stream_info(ptrInputFmtCtx, null).avThrowIfError();
            ffmpeg.av_dump_format(ptrInputFmtCtx, 0, string.Empty, 0);

            // Output: file
            var streamIndex = 0;
            var stream_mapping_size = (int)ptrInputFmtCtx->nb_streams;
            stream_mapping = (int*)ffmpeg.av_calloc((ulong)stream_mapping_size, 4);

            using var wStream = new UWriteStreamInternal(writeStream);
            avio_alloc_context_write_packet writeFn = wStream.WriteUnsafe;
            avio_alloc_context_seek seekFn2 = wStream.SeekUnsafe;
            var wBufLen = wStream.BufferLength;
            var ptrWBuffer = (byte*)ffmpeg.av_malloc((ulong)wBufLen);
            ptrOutputFmtCtx = ffmpeg.avformat_alloc_context();
            ////ffmpeg.avformat_alloc_output_context2(&ptrOutputFmtCtx, null, null, targetName).avThrowIfError();

            ptrOutputFmtCtx->pb = ffmpeg.avio_alloc_context(ptrWBuffer, wBufLen, 1, null, null, writeFn, seekFn2);
            ptrOutputFmtCtx->oformat = ffmpeg.av_guess_format(null, targetName, null);

            // For each input stream
            for (var i = 0; i < stream_mapping_size; i++)
            {
                AVStream* out_stream;
                var in_stream = ptrInputFmtCtx->streams[i];
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

            ffmpeg.av_dump_format(ptrOutputFmtCtx, 0, null, 1);

            // Write!
            ffmpeg.avformat_write_header(ptrOutputFmtCtx, null).avThrowIfError();

            var postHeaderSeeks = wStream.Seeks.ToList();
            var postHeaderWrites = wStream.Writes.ToList();

            while (true)
            {
                AVStream* in_stream;
                AVStream* out_stream;

                var read = ffmpeg.av_read_frame(ptrInputFmtCtx, ptrPacket);
                if (read == ffmpeg.AVERROR_EOF)
                {
                    break;
                }

                read.avThrowIfError();

                in_stream = ptrInputFmtCtx->streams[ptrPacket->stream_index];
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

            var postBodySeeks = wStream.Seeks.ToList();
            var postBodyWrites = wStream.Writes.ToList();
            wStream.Writes.Clear();

            ffmpeg.av_write_trailer(ptrOutputFmtCtx).avThrowIfError();

            var postTrailerSeeks = wStream.Seeks.ToList();
            var postTrailerOnlyWrites = wStream.Writes.ToList();

            return targetName;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
            throw;
        }
        finally
        {
            ffmpeg.av_packet_free(&ptrPacket);
            ffmpeg.avformat_close_input(&ptrInputFmtCtx);
            ffmpeg.avio_closep(&ptrOutputFmtCtx->pb);
            ffmpeg.avformat_free_context(ptrOutputFmtCtx);
            ffmpeg.av_freep(&stream_mapping);
        }
    }
}
