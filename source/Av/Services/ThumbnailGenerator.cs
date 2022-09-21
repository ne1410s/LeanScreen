using System.Collections.Generic;
using System.IO;
using Av.Models;
using FFMpegCore;

namespace Av.Services
{
    /// <inheritdoc cref="IThumbnailGenerator"/>
    public class ThumbnailGenerator : IThumbnailGenerator
    {
        /// <inheritdoc/>
        public void DumpEvenly(string filePath, int count)
        {
            var fi = new FileInfo(filePath);
            var analysis = FFProbe.Analyse(fi.FullName);
            var retval = new List<Thumb>();
            analysis.Duration.Distribute(count, (time, number) =>
            {
                var target = Path.Combine(fi.DirectoryName, $"snap_{number}_of_{count}.png");
                FFMpeg.Snapshot(fi.FullName, target, captureTime: time);
            });
        }
    }
}
