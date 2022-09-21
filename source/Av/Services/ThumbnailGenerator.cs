using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using Av.Models;
using FFMpegCore;

namespace Av.Services
{
    /// <inheritdoc cref="IThumbnailGenerator"/>
    public class ThumbnailGenerator : IThumbnailGenerator
    {
        /// <inheritdoc/>
        public void DumpCollation(
            string filePath,
            int count,
            int maxWidth,
            ISingleFilePathProvider pathProvider = null)
        {
            pathProvider = pathProvider ?? new DefaultSingleFilePathProvider();

            var fi = new FileInfo(filePath);
            var analysis = FFProbe.Analyse(fi.FullName);
            var targetPath = pathProvider.GetPath(fi, count);
            var size = maxWidth < analysis.PrimaryVideoStream.Width ? new Size { Width = maxWidth } : (Size?)null;

            // TODO: Remove this!
            var tempList = new List<TimeSpan>();

            analysis.Duration.Distribute(count, (time, number) =>
            {
                using (var bitmap = FFMpeg.Snapshot(fi.FullName, captureTime: time))
                {
                    // TODO: work out dimensions!!
                    var x = bitmap.Width;
                    tempList.Add(time);
                }
            });
        }

        /// <inheritdoc/>
        public void DumpMany(
            string filePath,
            int count,
            int maxWidth,
            ISequencedFilePathProvider pathProvider = null)
        {
            pathProvider = pathProvider ?? new DefaultSequencedFilePathProvider();

            var fi = new FileInfo(filePath);
            var analysis = FFProbe.Analyse(fi.FullName);
            var size = maxWidth < analysis.PrimaryVideoStream.Width ? new Size { Width = maxWidth } : (Size?)null;

            analysis.Duration.Distribute(count, (time, number) =>
            {
                var targetPath = pathProvider.GetPath(fi, number, count);
                FFMpeg.Snapshot(fi.FullName, targetPath, size, time);
            });
        }
    }
}