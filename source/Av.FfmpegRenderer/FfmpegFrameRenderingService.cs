using System;
using Av.Models;
using Av.Services;
using Crypt.Streams;

namespace Av.Renderer.Ffmpeg
{
    public class FfmpegFrameRenderingService : IVideoFrameRenderingService, IDisposable
    {
        private StreamSourceDecoder decoder;
        private Converter converter;

        public FfmpegFrameRenderingService()
        {
            FfmpegUtils.SetupBinaries();
            FfmpegUtils.SetupLogging();
        }

        public RenderingSessionInfo Load(ISimpleReadStream videoInput, Dimensions2D? itemSize = null)
        {
            converter?.Dispose();
            decoder?.Dispose();
            decoder = new StreamSourceDecoder(videoInput);
            converter = new Converter(decoder.Dimensions, decoder.PixelFormat, decoder.TimeBase, itemSize ?? decoder.Dimensions);
            return new RenderingSessionInfo
            {
                Duration = decoder.Duration,
                Dimensions = decoder.Dimensions,
            };
        }

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
