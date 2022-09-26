using System;
using System.Linq;

namespace Av
{
    /// <summary>
    /// Extensions to assist with the production of thumbnails.
    /// </summary>
    public static class ThumbingExtensions
    {
        /// <summary>
        /// Gets an evenly-distributed sequence of times.
        /// </summary>
        /// <param name="duration">The total duration.</param>
        /// <param name="count">The number of items to distribute.</param>
        /// <returns>A sequence of evenly-distributed times.</returns>
        public static TimeSpan[] DistributeEvenly(this TimeSpan duration, int count)
        {
            var deltaMs = duration.TotalMilliseconds / (Math.Max(2, count) - 1);
            return Enumerable.Range(0, count)
                .Select(n => TimeSpan.FromMilliseconds(deltaMs * n))
                .ToArray();
        }
    }
}
