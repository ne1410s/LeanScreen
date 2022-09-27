using System;
using Av.Abstractions.Models;
using Av.Abstractions.Rendering;
using Av.Abstractions.Shared;

namespace Av.Rendering.Ffmpeg
{
    public class FfmpegFrameRenderingService : IRenderingService, IDisposable
    {
        //private StreamSourceDecoder decoder;
        private PhysicalSourceDecoder decoder;
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
            //decoder = new StreamSourceDecoder(videoInput);
            decoder = new PhysicalSourceDecoder(videoInput);
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
