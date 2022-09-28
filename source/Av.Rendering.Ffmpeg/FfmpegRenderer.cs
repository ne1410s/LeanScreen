using System;
using Av.Abstractions.Rendering;
using Av.Abstractions.Shared;
using Crypt.Streams;

namespace Av.Rendering.Ffmpeg
{
    public class FfmpegRenderer : IRenderingService, IDisposable
    {
        private readonly IDecoder decoder;
        private readonly Converter converter;
        private readonly ISimpleReadStream inputStream;

        public FfmpegRenderer(string input, Dimensions2D? frameSize = null)
        {
            FfmpegUtils.SetupBinaries();
            FfmpegUtils.SetupLogging();
            decoder = new PhysicalSourceDecoder(input);
            FrameSize = frameSize ?? decoder.Dimensions;
            Duration = decoder.Duration;
            TotalFrames = decoder.TotalFrames;
            converter = new Converter(decoder.Dimensions, decoder.PixelFormat, FrameSize);
        }

        public FfmpegRenderer(ISimpleReadStream input, Dimensions2D? frameSize = null)
        {
            inputStream = input;
            FfmpegUtils.SetupBinaries();
            FfmpegUtils.SetupLogging();
            decoder = new StreamSourceDecoder(input);
            FrameSize = frameSize ?? decoder.Dimensions;
            Duration = decoder.Duration;
            TotalFrames = decoder.TotalFrames;
            converter = new Converter(decoder.Dimensions, decoder.PixelFormat, FrameSize);
        }

        public TimeSpan Duration { get; }

        public Dimensions2D FrameSize { get; }

        public long TotalFrames { get; }

        public RenderedFrame RenderAt(TimeSpan position)
        {
            decoder.Seek(position.Clamp(decoder.Duration));
            decoder.TryDecodeNextFrame(out var frame);
            var rawFrame = converter.RenderRawFrame(frame);
            var actualPosition = rawFrame.PresentationTime.ToTimeSpan(decoder.TimeBase);
            var inferredFrame = TotalFrames * (actualPosition.TotalSeconds / Duration.TotalSeconds);

            return new RenderedFrame
            {
                Rgb24Bytes = rawFrame.Rgb24Bytes,
                Dimensions = FrameSize,
                Position = actualPosition,
                FrameNumber = (long)Math.Round(inferredFrame),
            };
        }

        public void Dispose()
        {
            converter?.Dispose();
            decoder?.Dispose();
            inputStream?.Dispose();
        }
    }
}
