// <copyright file="FfmpegFormatConverter.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace LeanScreen.Rendering.Ffmpeg.Conversion;

using System;
using System.IO;
using CryptoStream.IO;
using CryptoStream.Streams;
using FFmpeg.AutoGen;
using LeanScreen.Rendering.Ffmpeg.IO;

/// <summary>
/// Converts media formats using ffmpeg.
/// </summary>
public unsafe class FfmpegFormatConverter
{
    /// <summary>
    /// Remultiplexes the source into the desired format.
    /// </summary>
    /// <param name="source">The source file.</param>
    /// <param name="targetExtension">The target extension.</param>
    /// <param name="userKey">The key.</param>
    /// <param name="dbgFSO">DEBUG ONLY: uses bare file target.</param>
    /// <returns>The converted file.</returns>
    public FileInfo Remux(
        FileInfo source,
        string targetExtension,
        byte[] userKey,
        bool dbgFSO = false)
    {
        if (source == null)
        {
            throw new ArgumentNullException(nameof(source));
        }

        var secure = source.IsSecure();
        var post = dbgFSO ? "fs" : "bs";
        var targetName = $"{source.Name}__bs2{post}{targetExtension}";
        var targetPath = Path.Combine(source.DirectoryName, "out", targetName);
        var target = new FileInfo(targetPath);
        var salt = secure ? source.ToSalt() : [];

        using BlockStream inputStream = secure
            ? source.OpenCryptoRead(userKey)
            : source.OpenBlockRead();

        using Stream outputStream = dbgFSO
            ? target.OpenWrite()
            : secure
                ? target.OpenCryptoWrite(salt, userKey, targetExtension)
                : target.OpenBlockWrite();

        FfmpegUtils.SetBinariesPath();
        FfmpegUtils.SetupLogging();

        AVPacket* ptrPacket = null;
        AVFormatContext* ptrInputFmtCtx = null;
        AVFormatContext* ptrOutputFmtCtx = null;
        int* stream_mapping = null;

        try
        {
            // FILE input be like:
            ////ffmpeg.avformat_open_input(&ptrInputFmtCtx, source.FullName, null, null).avThrowIfError();

            // STREAM input be like:
            using var ffmpegReadStream = new FfmpegUStream(inputStream);
            avio_alloc_context_read_packet readFn = ffmpegReadStream.ReadUnsafe;
            avio_alloc_context_seek seekFn = ffmpegReadStream.SeekUnsafe;
            var bufLen = ffmpegReadStream.BufferLength;
            var ptrBuffer = (byte*)ffmpeg.av_malloc((ulong)bufLen);
            ptrInputFmtCtx = ffmpeg.avformat_alloc_context();
            ptrInputFmtCtx->pb = ffmpeg.avio_alloc_context(ptrBuffer, bufLen, 0, null, readFn, null, seekFn);
            ffmpeg.avformat_open_input(&ptrInputFmtCtx, string.Empty, null, null).avThrowIfError();

            // NOTE: Strwip (S2F) seems fine! so why does this approach add like 1000 mb to file???!

            // Process
            ptrPacket = ffmpeg.av_packet_alloc();
            ffmpeg.avformat_find_stream_info(ptrInputFmtCtx, null).avThrowIfError();
            ffmpeg.av_dump_format(ptrInputFmtCtx, 0, string.Empty, 0);

            // FILE output be like:
            ////ffmpeg.avformat_alloc_output_context2(&ptrOutputFmtCtx, null, null, target.FullName).avThrowIfError();
            ////if ((ptrOutputFmtCtx->oformat->flags & ffmpeg.AVFMT_NOFILE) == 0)
            ////{
            ////    // technically this bit was just before avformat_write_header()
            ////    ffmpeg.avio_open(&ptrOutputFmtCtx->pb, target.FullName, ffmpeg.AVIO_FLAG_WRITE).avThrowIfError();
            ////}

            // STREAM output be like:
            using var ffmpegWriteStream = new FfmpegUStream(outputStream);
            avio_alloc_context_write_packet writeFn = ffmpegWriteStream.WriteUnsafe;
            avio_alloc_context_seek seekFn2 = ffmpegWriteStream.SeekUnsafe;
            var wBufLen = ffmpegWriteStream.BufferLength;
            var ptrWBuffer = (byte*)ffmpeg.av_malloc((ulong)wBufLen);
            ptrOutputFmtCtx = ffmpeg.avformat_alloc_context();
            ptrOutputFmtCtx->pb = ffmpeg.avio_alloc_context(ptrWBuffer, wBufLen, 1, null, null, writeFn, seekFn2);
            ptrOutputFmtCtx->oformat = ffmpeg.av_guess_format(null, targetName, null);

            // Media channels (aka ffmpeg "streams")
            var streamIndex = 0;
            var stream_mapping_size = (int)ptrInputFmtCtx->nb_streams;
            stream_mapping = (int*)ffmpeg.av_calloc((ulong)stream_mapping_size, 4);
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

            // Enable buffer mode
            ////ffmpegWriteStream.BufferTrailerMode = true;

            ffmpeg.av_write_trailer(ptrOutputFmtCtx).avThrowIfError();

            if (outputStream is BlockStream bs)
            {
                bs.FinaliseWrite();
                if (bs is GcmCryptoStream)
                {
                    target.MoveTo(Path.Combine(target.DirectoryName, bs.Id));
                }
            }

            return target;
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
