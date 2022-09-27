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

        public FfmpegRenderer(string input, Dimensions2D? frameSize = null)
        {
            FfmpegUtils.SetupBinaries();
            FfmpegUtils.SetupLogging();
            decoder = new PhysicalSourceDecoder(input);
            FrameSize = frameSize ?? decoder.Dimensions;
            Duration = decoder.Duration;
            converter = new Converter(decoder.Dimensions, decoder.PixelFormat, decoder.TimeBase, FrameSize);
        }

        public FfmpegRenderer(ISimpleReadStream input, Dimensions2D? frameSize = null)
        {
            FfmpegUtils.SetupBinaries();
            FfmpegUtils.SetupLogging();
            decoder = new StreamSourceDecoder(input);
            FrameSize = frameSize ?? decoder.Dimensions;
            Duration = decoder.Duration;
            converter = new Converter(decoder.Dimensions, decoder.PixelFormat, decoder.TimeBase, FrameSize);
        }

        public Dimensions2D FrameSize { get; }

        public TimeSpan Duration { get; }

        public RenderedFrame RenderAt(TimeSpan position)
        {
            decoder.Seek(position.Clamp(decoder.Duration));
            decoder.TryDecodeNextFrame(out var frame);
            return converter.Render(frame);
        }

        public void Dispose()
        {
            converter?.Dispose();
            decoder?.Dispose();
        }
    }
}
