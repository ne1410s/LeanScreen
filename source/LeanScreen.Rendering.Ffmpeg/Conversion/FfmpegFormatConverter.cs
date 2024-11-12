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
public static unsafe class FfmpegFormatConverter
{
    /// <summary>
    /// Remultiplexes the source into the desired format.
    /// </summary>
    /// <param name="source">The source file.</param>
    /// <param name="ext">The target extension.</param>
    /// <param name="key">The key.</param>
    /// <param name="directFile">If true then ffmpeg is sent only file path(s)
    /// from which format contexts are populated, instead of streams.</param>
    /// <returns>The converted file.</returns>
    public static FileInfo Remux(FileInfo source, string ext, byte[] key, bool directFile = false)
    {
        source = source ?? throw new ArgumentNullException(nameof(source));
        var isSecure = source.IsSecure();
        if (isSecure && directFile)
        {
            throw new ArgumentException("Direct file mode not supported for secure sources", nameof(directFile));
        }

        var mid = directFile ? "F2F" : "B2B";
        var targetName = $"{source.Name}__{mid}{ext}";
        var targetPath = Path.Combine(source.DirectoryName, targetName);
        var target = new FileInfo(targetPath);

        FfmpegUtils.SetBinariesPath();
        FfmpegUtils.SetupLogging();

        AVPacket* ptrPacket = null;
        AVFormatContext* ptrInputFmtCtx = null;
        AVFormatContext* ptrOutputFmtCtx = null;
        int* stream_mapping = null;

        BlockStream inputStream = null!;
        BlockStream outputStream = null!;
        FfmpegUStream ffmpegReadStream = null!;
        FfmpegUStream ffmpegWriteStream = null!;

        try
        {
            if (directFile)
            {
                // Populate input format context from file path
                ffmpeg.avformat_open_input(&ptrInputFmtCtx, source.FullName, null, null).avThrowIfError();
            }
            else
            {
                // Populate input format context from stream
                inputStream = source.IsSecure()
                    ? source.OpenCryptoRead(key)
                    : source.OpenBlockRead();
                ffmpegReadStream = new FfmpegUStream(inputStream);
                avio_alloc_context_read_packet readFn = ffmpegReadStream.ReadUnsafe;
                avio_alloc_context_seek seekFn = ffmpegReadStream.SeekUnsafe;
                var bufLen = ffmpegReadStream.BufferLength;
                var ptrBuffer = (byte*)ffmpeg.av_malloc((ulong)bufLen);
                ptrInputFmtCtx = ffmpeg.avformat_alloc_context();
                ptrInputFmtCtx->pb = ffmpeg.avio_alloc_context(ptrBuffer, bufLen, 0, null, readFn, null, seekFn);
                ffmpeg.avformat_open_input(&ptrInputFmtCtx, string.Empty, null, null).avThrowIfError();
            }

            // Process
            ptrPacket = ffmpeg.av_packet_alloc();
            ffmpeg.avformat_find_stream_info(ptrInputFmtCtx, null).avThrowIfError();
            ffmpeg.av_dump_format(ptrInputFmtCtx, 0, string.Empty, 0);

            if (directFile)
            {
                // Populate output format context from file path
                ffmpeg.avformat_alloc_output_context2(&ptrOutputFmtCtx, null, null, target.FullName).avThrowIfError();
                if ((ptrOutputFmtCtx->oformat->flags & ffmpeg.AVFMT_NOFILE) == 0)
                {
                    ffmpeg.avio_open(&ptrOutputFmtCtx->pb, target.FullName, ffmpeg.AVIO_FLAG_WRITE).avThrowIfError();
                }
            }
            else
            {
                // Populate output format context from stream
                outputStream = source.IsSecure()
                    ? target.OpenCryptoWrite(source.ToSalt(), key, target.Extension)
                    : target.OpenBlockWrite();
                ffmpegWriteStream = new FfmpegUStream(outputStream);
                avio_alloc_context_write_packet writeFn = ffmpegWriteStream.WriteUnsafe;
                avio_alloc_context_seek seekFn2 = ffmpegWriteStream.SeekUnsafe;
                var wBufLen = ffmpegWriteStream.BufferLength;
                var ptrWBuffer = (byte*)ffmpeg.av_malloc((ulong)wBufLen);
                ptrOutputFmtCtx = ffmpeg.avformat_alloc_context();
                ptrOutputFmtCtx->pb = ffmpeg.avio_alloc_context(ptrWBuffer, wBufLen, 1, null, null, writeFn, seekFn2);
                ptrOutputFmtCtx->oformat = ffmpeg.av_guess_format(null, target.FullName, null);
            }

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

            if (!directFile)
            {
                // Enable trailer cache!
                outputStream.CacheTrailer = true;
            }

            ffmpeg.av_write_trailer(ptrOutputFmtCtx).avThrowIfError();

            outputStream?.FinaliseWrite();
            outputStream?.Close();

            if (outputStream is GcmCryptoStream)
            {
                var moveTo = Path.Combine(target.DirectoryName, outputStream.Id);
                File.Delete(moveTo);
                target.MoveTo(moveTo);
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

            ffmpegWriteStream?.Dispose();
            outputStream?.Dispose();
            ffmpegReadStream?.Dispose();
            inputStream?.Dispose();
        }
    }
}
