// <copyright file="FfmpegFormatConverter.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace LeanScreen.Rendering.Ffmpeg.Conversion;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using CryptoStream.IO;
using CryptoStream.Streams;
using FFmpeg.AutoGen;
using LeanScreen.Abstractions.Conversion;
using LeanScreen.Rendering.Ffmpeg.IO;

/// <inheritdoc cref="IFormatConverter"/>
public unsafe class FfmpegFormatConverter : IFormatConverter
{
    /// <inheritdoc/>
    public FileInfo Remux(FileInfo source, string ext, byte[] key, bool directFile = false)
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

        BlockStream inputStream = null!;
        BlockStream outputStream = null!;
        FfmpegUStream ffmpegReadStream = null!;
        FfmpegUStream ffmpegWriteStream = null!;

        // This from some example I found once
        //// For sample video: sample-10s.mp4
        //// avi: H.264 bitstream malformed, no startcode found, use the video bitstream filter 'h264_mp4toannexb' to fix it ('-bsf:v h264_mp4toannexb' option with ffmpeg)
        ////      Error muxing packet
        ////      Error occurred: Invalid data found when processing input
        //// vob: [svcd @ 000001ecb820b380] VBV buffer size not set, using default size of 230KB
        ////      If you want the mpeg file to be compliant to some specification
        ////      Like DVD, VCD or others, make sure you set the correct buffer size
        ////      [svcd @ 000001ecb820b380] Unsupported audio codec.Must be one of mp1, mp2, mp3, 16 - bit pcm_dvd, pcm_s16be, ac3 or dts.
        ////      Error occurred when opening output file
        ////      Error occurred: Invalid argument
        //// mpeg: [mpeg @ 0000020de269b380] VBV buffer size not set, using default size of 230KB
        ////      If you want the mpeg file to be compliant to some specification
        ////      Like DVD, VCD or others, make sure you set the correct buffer size
        ////      [mpeg @ 0000020de269b380] Unsupported audio codec.Must be one of mp1, mp2, mp3, 16 - bit pcm_dvd, pcm_s16be, ac3 or dts.
        ////      Error occurred when opening output file
        ////      Error occurred: Invalid argument
        //// These are OK!
        //// mkv, asf, mov, vob, flv, mp4, ts

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
            var usefulStreamTypes = new HashSet<AVMediaType>(
            [
                AVMediaType.AVMEDIA_TYPE_AUDIO,
                AVMediaType.AVMEDIA_TYPE_VIDEO,
                AVMediaType.AVMEDIA_TYPE_SUBTITLE,
            ]);

            var stream_mapping = Enumerable.Range(0, stream_mapping_size).ToList();
            for (var i = 0; i < stream_mapping_size; i++)
            {
                AVStream* out_stream;
                var in_stream = ptrInputFmtCtx->streams[i];
                var in_codecpar = in_stream->codecpar;
                if (usefulStreamTypes.Contains(in_codecpar->codec_type))
                {
                    stream_mapping[i] = streamIndex++;
                    out_stream = ffmpeg.avformat_new_stream(ptrOutputFmtCtx, null);
                    ffmpeg.avcodec_parameters_copy(out_stream->codecpar, in_codecpar).avThrowIfError();
                    out_stream->codecpar->codec_tag = 0;
                }
            }

            ffmpeg.av_dump_format(ptrOutputFmtCtx, 0, null, 1);

            // Write!
            ffmpeg.avformat_write_header(ptrOutputFmtCtx, null).avThrowIfError();

            while (true)
            {
                var read = ffmpeg.av_read_frame(ptrInputFmtCtx, ptrPacket);
                if (read == ffmpeg.AVERROR_EOF)
                {
                    break;
                }

                read.avThrowIfError();
                var in_stream = ptrInputFmtCtx->streams[ptrPacket->stream_index];
                WritePacket(stream_mapping, ptrOutputFmtCtx, in_stream, ptrPacket);
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
#if DEBUG
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
            throw;
        }
#endif
        finally
        {
            ffmpeg.av_packet_free(&ptrPacket);
            ffmpeg.avformat_close_input(&ptrInputFmtCtx);
            ffmpeg.avio_closep(&ptrOutputFmtCtx->pb);
            ffmpeg.avformat_free_context(ptrOutputFmtCtx);

            ffmpegWriteStream?.Dispose();
            outputStream?.Dispose();
            ffmpegReadStream?.Dispose();
            inputStream?.Dispose();
        }
    }

    [ExcludeFromCodeCoverage]
    private static void WritePacket(
        List<int> streamMapping, AVFormatContext* ptrOutputFmtCtx, AVStream* in_stream, AVPacket* ptrPacket)
    {
        if (!streamMapping.Contains(ptrPacket->stream_index))
        {
            ffmpeg.av_packet_unref(ptrPacket);
        }
        else
        {
            ptrPacket->stream_index = streamMapping[ptrPacket->stream_index];
            var out_stream = ptrOutputFmtCtx->streams[ptrPacket->stream_index];
            ffmpeg.av_packet_rescale_ts(ptrPacket, in_stream->time_base, out_stream->time_base);
            ptrPacket->pos = -1;
            ffmpeg.av_interleaved_write_frame(ptrOutputFmtCtx, ptrPacket).avThrowIfError();
        }
    }
}
