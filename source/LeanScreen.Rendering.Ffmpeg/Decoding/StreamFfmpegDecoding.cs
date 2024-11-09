// <copyright file="StreamFfmpegDecoding.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace LeanScreen.Rendering.Ffmpeg.Decoding;

using CryptoStream.Streams;
using FFmpeg.AutoGen;
using LeanScreen.Rendering.Ffmpeg.IO;

/// <summary>
/// Ffmpeg decoding session for stream sources.
/// </summary>
public sealed unsafe class StreamFfmpegDecoding : FfmpegDecodingSessionBase
{
    private readonly BlockStream readStream;
    private FfmpegUStream? uStream;
    private avio_alloc_context_read_packet? readFn;
    private avio_alloc_context_seek? seekFn;
    private AVIOContext* streamIc;

    /// <summary>
    /// Initializes a new instance of the <see cref="StreamFfmpegDecoding"/> class.
    /// </summary>
    /// <param name="stream">A stream.</param>
    public StreamFfmpegDecoding(BlockStream stream)
        : base(string.Empty)
    {
        this.readStream = stream;
        this.uStream = new FfmpegUStream(stream);
        this.readFn = this.uStream.ReadUnsafe;
        this.seekFn = this.uStream.SeekUnsafe;
        var bufLen = this.uStream.BufferLength;
        var ptrBuffer = (byte*)ffmpeg.av_malloc((ulong)bufLen);
        this.streamIc = ffmpeg.avio_alloc_context(ptrBuffer, bufLen, 0, null, this.readFn, null, this.seekFn);

        // NB: This creates memory issue. .. So it's blindly commented out :D
        ////streamIc->seekable = 1;

        PtrFormatContext->pb = this.streamIc;

        this.OpenInputContext();
    }

    /// <inheritdoc/>
    public override void Dispose()
    {
        base.Dispose();

        ffmpeg.av_freep(&streamIc->buffer);
        var customInputContext = this.streamIc;
        ffmpeg.av_freep(&customInputContext);
        this.streamIc = null;
        this.readFn = null;
        this.seekFn = null;
        this.uStream!.Dispose();
        this.uStream = null;
        this.readStream.Dispose();
    }
}
