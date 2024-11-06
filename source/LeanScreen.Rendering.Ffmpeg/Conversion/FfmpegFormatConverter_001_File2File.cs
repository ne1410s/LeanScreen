namespace LeanScreen.Rendering.Ffmpeg.Conversion;

using System;
using System.IO;
using FFmpeg.AutoGen;

/// <summary>
/// Remuxer for Ffmpeg.
/// </summary>
public class FfmpegFormatConverter_001_File2File
{
    /// <summary>
    /// Re-multiplexes streams to another container format (without transcoding).
    /// </summary>
    /// <returns>Some number.</returns>
    public static unsafe int Remux(string fileName, string targetExt)
    {
        FfmpegUtils.SetBinariesPath();

        AVOutputFormat* ofmt = null;
        AVFormatContext* ifmt_ctx = null;
        AVFormatContext* ofmt_ctx = null;
        AVPacket* pkt = null;
        string out_filename;
        int ret, i;
        int stream_index = 0;
        int* stream_mapping = null;
        int stream_mapping_size = 0;

        // For sample video: sample-10s.mp4

        // avi: H.264 bitstream malformed, no startcode found, use the video bitstream filter 'h264_mp4toannexb' to fix it ('-bsf:v h264_mp4toannexb' option with ffmpeg)
        //      Error muxing packet
        //      Error occurred: Invalid data found when processing input
        // vob: [svcd @ 000001ecb820b380] VBV buffer size not set, using default size of 230KB
        //      If you want the mpeg file to be compliant to some specification
        //      Like DVD, VCD or others, make sure you set the correct buffer size
        //      [svcd @ 000001ecb820b380] Unsupported audio codec.Must be one of mp1, mp2, mp3, 16 - bit pcm_dvd, pcm_s16be, ac3 or dts.
        //      Error occurred when opening output file
        //      Error occurred: Invalid argument
        // mpeg: [mpeg @ 0000020de269b380] VBV buffer size not set, using default size of 230KB
        //      If you want the mpeg file to be compliant to some specification
        //      Like DVD, VCD or others, make sure you set the correct buffer size
        //      [mpeg @ 0000020de269b380] Unsupported audio codec.Must be one of mp1, mp2, mp3, 16 - bit pcm_dvd, pcm_s16be, ac3 or dts.
        //      Error occurred when opening output file
        //      Error occurred: Invalid argument

        // These are OK!
        // mkv, asf, mov, vob, flv, mp4, ts
        out_filename = $@"C:\temp\~vids\out\111_demo{targetExt}";

        pkt = ffmpeg.av_packet_alloc();
        if (pkt == null)
        {
            Console.WriteLine("Could not allocate AVPacket");
            return 1;
        }

        if ((ret = ffmpeg.avformat_open_input(&ifmt_ctx, fileName, null, null)) < 0)
        {
            Console.WriteLine($"Could not open input file");
            goto end;
        }

        if ((ret = ffmpeg.avformat_find_stream_info(ifmt_ctx, null)) < 0)
        {
            Console.WriteLine("Failed to retrieve input stream information");
            goto end;
        }

        ffmpeg.av_dump_format(ifmt_ctx, 0, fileName, 0);

        ffmpeg.avformat_alloc_output_context2(&ofmt_ctx, null, null, out_filename);
        if (ofmt_ctx == null)
        {
            Console.WriteLine("Could not create output context");
            ret = ffmpeg.AVERROR_UNKNOWN;
            goto end;
        }

        stream_mapping_size = (int)ifmt_ctx->nb_streams;
        stream_mapping = (int*)ffmpeg.av_calloc((ulong)stream_mapping_size, 4);
        if (stream_mapping == null)
        {
            ret = ffmpeg.AVERROR(ffmpeg.ENOMEM);
            goto end;
        }

        ofmt = ofmt_ctx->oformat;

        for (i = 0; i < ifmt_ctx->nb_streams; i++)
        {
            AVStream* out_stream;
            AVStream* in_stream = ifmt_ctx->streams[i];
            AVCodecParameters* in_codecpar = in_stream->codecpar;

            if (in_codecpar->codec_type != AVMediaType.AVMEDIA_TYPE_AUDIO &&
                in_codecpar->codec_type != AVMediaType.AVMEDIA_TYPE_VIDEO &&
                in_codecpar->codec_type != AVMediaType.AVMEDIA_TYPE_SUBTITLE)
            {
                stream_mapping[i] = -1;
                continue;
            }

            stream_mapping[i] = stream_index++;

            out_stream = ffmpeg.avformat_new_stream(ofmt_ctx, null);
            if (out_stream == null)
            {
                Console.WriteLine("Failed allocating output stream");
                ret = ffmpeg.AVERROR_UNKNOWN;
                goto end;
            }

            ret = ffmpeg.avcodec_parameters_copy(out_stream->codecpar, in_codecpar);
            if (ret < 0)
            {
                Console.WriteLine("Failed to copy codec parameters");
                goto end;
            }

            out_stream->codecpar->codec_tag = 0;
        }

        ffmpeg.av_dump_format(ofmt_ctx, 0, out_filename, 1);

        if ((ofmt->flags & ffmpeg.AVFMT_NOFILE) == 0)
        {
            ret = ffmpeg.avio_open(&ofmt_ctx->pb, out_filename, ffmpeg.AVIO_FLAG_WRITE);
            if (ret < 0)
            {
                Console.WriteLine($"Could not open output file '{out_filename}");
                goto end;
            }
        }

        ret = ffmpeg.avformat_write_header(ofmt_ctx, null);
        if (ret < 0)
        {
            Console.WriteLine("Error occurred when opening output file");
            goto end;
        }

        while (true)
        {
            AVStream* in_stream;
            AVStream* out_stream;

            ret = ffmpeg.av_read_frame(ifmt_ctx, pkt);
            if (ret < 0)
            {
                break;
            }

            in_stream = ifmt_ctx->streams[pkt->stream_index];
            if (pkt->stream_index >= stream_mapping_size || stream_mapping[pkt->stream_index] < 0)
            {
                ffmpeg.av_packet_unref(pkt);
                continue;
            }

            //// Errrrr maybe somat like this?
            //if (in_stream->codecpar->codec_type == AVMediaType.AVMEDIA_TYPE_VIDEO)
            //{
            //    ret = ffmpeg.avformat_match_stream_specifier(ofmt_ctx, in_stream, "h264_mp4toannexb");
            //    if (ret < 0)
            //    {
            //        goto end;
            //    }
            //}

            pkt->stream_index = stream_mapping[pkt->stream_index];
            out_stream = ofmt_ctx->streams[pkt->stream_index];

            ffmpeg.av_packet_rescale_ts(pkt, in_stream->time_base, out_stream->time_base);
            pkt->pos = -1;

            ret = ffmpeg.av_interleaved_write_frame(ofmt_ctx, pkt);
            if (ret < 0)
            {
                Console.WriteLine("Error muxing packet");
                break;
            }
        }

        ffmpeg.av_write_trailer(ofmt_ctx);

    end:
        ffmpeg.av_packet_free(&pkt);
        ffmpeg.avformat_close_input(&ifmt_ctx);

        if (ofmt_ctx != null && (ofmt->flags & ffmpeg.AVFMT_NOFILE) == 0)
        {
            ffmpeg.avio_closep(&ofmt_ctx->pb);
        }

        ffmpeg.avformat_free_context(ofmt_ctx);

        ffmpeg.av_freep(&stream_mapping);

        if (ret < 0 && ret != ffmpeg.AVERROR_EOF)
        {
            var errorText = FfmpegUtils.AvStrError(ret);
            Console.WriteLine($"Error occurred: {errorText}");
            return 1;
        }

        return 0;
    }
}
