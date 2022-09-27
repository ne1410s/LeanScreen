using System;
using System.Linq;
using Av.Abstractions.Rendering;

namespace Av.Services
{
    /// <summary>
    /// Generates thumbnails.
    /// </summary>
    public class ThumbnailGenerator
    {
        private readonly IRenderingService renderer;

        /// <summary>
        /// Initialises a new <see cref="ThumbnailGenerator"/>.
        /// </summary>
        /// <param name="renderer">A video renderer.</param>
        public ThumbnailGenerator(IRenderingService renderer)
        {
            this.renderer = renderer;
        }

        /// <summary>
        /// Provides access for the caller to handle rendered frames directly,
        /// over an evenly-distributed sequence of times.
        /// </summary>
        /// <param name="onRendered">On rendered callback.</param>
        /// <param name="itemCount">The number to generate.</param>
        public void Generate(
            Action<RenderedFrame, int> onRendered,
            int itemCount = 24)
                => Generate(onRendered, new TimeSpan[itemCount]);

        /// <summary>
        /// Provides access for the caller to handle rendered frames directly,
        /// over a sequence of specific times.
        /// </summary>
        /// <param name="onRendered">On rendered callback.</param>
        /// <param name="times">A sequence of times.</param>
        public void Generate(
            Action<RenderedFrame, int> onRendered,
            params TimeSpan[] times)
        {
            if (times.Length > 1 && times.All(t => t == default))
            {
                times = renderer.Duration.DistributeEvenly(times.Length);
            }

            for (var i = 0; i < times.Length; i++)
            {
                var frame = renderer.RenderAt(times[i]);
                onRendered.Invoke(frame, i);
            }
        }
    }
}