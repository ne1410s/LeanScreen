using System;

namespace Av
{
    /// <summary>
    /// Extensions to assist with the production of thumbnails.
    /// </summary>
    public static class ThumbingExtensions
    {
        /// <summary>
        /// Performs an action for an evenly-distributed sequence of times.
        /// </summary>
        /// <param name="duration">The total duration.</param>
        /// <param name="count">The number of times to perform an action.</param>
        /// <param name="action">The action callback.</param>
        public static void Distribute(this TimeSpan duration, int count, Action<TimeSpan, int> action)
        {
            var deltaMs = duration.TotalMilliseconds / (count - 1);
            for (var i = 0; i < count; i++)
            {
                var time = TimeSpan.FromMilliseconds(deltaMs * i);
                action.Invoke(time, i + 1);
            }
        }
    }
}
