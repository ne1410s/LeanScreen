using System;
using Av.Models;
using Av.Services;

namespace Av.Renderer.Ffmpeg
{
    public class FfmpegFrameRenderingService : IVideoFrameRenderingService, IDisposable
    {
        private Decoder decoder;
        private Converter converter;

        public FfmpegFrameRenderingService()
        {
            FfmpegUtils.SetupBinaries();
            FfmpegUtils.SetupLogging();
        }

        public RenderingSessionInfo Load(string videoInput, Dimensions2D? itemSize = null)
        {
            converter?.Dispose();
            decoder?.Dispose();
            decoder = new Decoder(videoInput);
            converter = new Converter(decoder.Dimensions, decoder.PixelFormat, decoder.TimeBase, itemSize ?? decoder.Dimensions);
            return new RenderingSessionInfo
            {
                Duration = decoder.Duration,
                Dimensions = decoder.Dimensions,
            };
        }

        public RenderedFrame RenderAt(TimeSpan position)
        {
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
