using System;
using System.IO;
using System.Linq;
using Av.Models;
using Crypt.Streams;

namespace Av.Services
{
    /// <summary>
    /// Generates thumbnails.
    /// </summary>
    public class ThumbnailGenerator
    {
        private readonly IVideoFrameRenderingService renderer;

        /// <summary>
        /// Initialises a new <see cref="ThumbnailGenerator"/>.
        /// </summary>
        /// <param name="renderer">A video renderer.</param>
        public ThumbnailGenerator(IVideoFrameRenderingService renderer)
        {
            this.renderer = renderer;
        }

        /// <summary>
        /// Provides access for the caller to handle rendered frames directly,
        /// over an evenly-distributed sequence of times.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <param name="onRendered">On rendered callback.</param>
        /// <param name="itemSize">The desired target dimensions.</param>
        /// <param name="itemCount">The number to generate.</param>
        public void Generate(
            string filePath,
            Action<RenderedFrame, int> onRendered,
            Dimensions2D? itemSize = null,
            int itemCount = 24)
                => Generate(filePath, onRendered, itemSize, new TimeSpan[itemCount]);

        /// <summary>
        /// Provides access for the caller to handle rendered frames directly,
        /// over a sequence of specific times.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <param name="onRendered">On rendered callback.</param>
        /// <param name="itemSize">The desired target dimensions.</param>
        /// <param name="times">A sequence of times.</param>
        public void Generate(
            string filePath,
            Action<RenderedFrame, int> onRendered,
            Dimensions2D? itemSize = null,
            params TimeSpan[] times)
        {
            var blockStream = new BlockReadStream(new FileInfo(filePath));
            var info = renderer.Load(blockStream, itemSize);
            if (times.Length > 1 && times.All(t => t == default))
            {
                times = info.Duration.DistributeEvenly(times.Length);
            }

            for (var i = 0; i < times.Length; i++)
            {
                var frame = renderer.RenderAt(times[i]);
                onRendered.Invoke(frame, i);
            }
        }
    }
}