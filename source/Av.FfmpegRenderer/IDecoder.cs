using System;
using Av.Abstractions.Shared;
using FFmpeg.AutoGen;

namespace Av.Rendering.Ffmpeg
{
    public interface IDecoder : IDisposable
    {
        string CodecName { get; }

        Dimensions2D Dimensions { get; }

        TimeSpan Duration { get; }

        AVPixelFormat PixelFormat { get; }

        AVRational TimeBase { get; }

        long TotalFrames { get; }

        void Seek(TimeSpan position);

        bool TryDecodeNextFrame(out AVFrame frame);
    }
}
