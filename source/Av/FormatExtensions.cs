using System;
using System.Collections.Generic;
using Av.Models;

namespace Av
{
    /// <summary>
    /// Extensions for formatting.
    /// </summary>
    public static class FormatExtensions
    {
        private const double Kilobyte = 1024;
        private const double Megabyte = 1048576;
        private const double Gigabyte = 1073741824;
        private const double Terabyte = 1099511627776;

        private static readonly IReadOnlyDictionary<string, string> ArchiveMimes =
            new Dictionary<string, string>
            {
                { ".7z", "application/x-7z-compressed" },
                { ".gz", "application/gzip" },
                { ".jar", "application/java-archive" },
                { ".rar", "application/vnd.rar" },
                { ".tar", "application/x-tar" },
                { ".zip", "application/zip" },
            };

        private static readonly IReadOnlyDictionary<string, string> ImageMimes =
            new Dictionary<string, string>
            {
                { ".bmp", "image/bmp" },
                { ".gif", "image/gif" },
                { ".jpe", "image/jpeg" },
                { ".jpeg", "image/jpeg" },
                { ".jpg", "image/jpeg" },
                { ".png", "image/png" },
                { ".tif", "image/tiff" },
                { ".tiff", "image/tiff" },
                { ".webp", "image/webp" },
            };

        private static readonly IReadOnlyDictionary<string, string> AudioMimes =
            new Dictionary<string, string>
            {
                { ".aac", "audio/aac" },
                { ".m4a", "audio/m4a" },
                { ".mp3", "audio/mpeg" },
                { ".oga", "audio/ogg" },
                { ".wav", "audio/wav" },
                { ".weba", "audio/webm" },
            };

        private static readonly IReadOnlyDictionary<string, string> VideoMimes =
            new Dictionary<string, string>
            {
                { ".3g2", "video/3gpp2" },
                { ".3gp", "video/3gpp" },
                { ".avi", "video/x-msvideo" },
                { ".flv", "video/x-flv" },
                { ".m2v", "video/mpeg" },
                { ".m4v", "video/x-m4v" },
                { ".mkv", "video/x-matroska" },
                { ".mov", "video/quicktime" },
                { ".mp4", "video/mp4" },
                { ".mpeg", "video/mpeg" },
                { ".mpg", "video/mpeg" },
                { ".mts", "video/mp2t" },
                { ".swf", "application/x-shockwave-flash" },
                { ".ts", "video/mp2t" },
                { ".ogg", "video/ogg" },
                { ".ogv", "video/ogg" },
                { ".vob", "video/x-ms-vob" },
                { ".webm", "video/webm" },
                { ".wmv", "video/x-ms-wmv" },
            };

        /// <summary>
        /// Formats a byte count as size on disk.
        /// </summary>
        /// <param name="bytes">The byte count.</param>
        /// <returns>Size on disk.</returns>
        public static string FormatSize(this long bytes) =>
            bytes < Kilobyte ? $"{bytes}b"
                : bytes < Megabyte ? $"{bytes / Kilobyte:N0}kb"
                : bytes < Gigabyte ? $"{bytes / Megabyte:N0}mb"
                : bytes < Terabyte ? $"{bytes / Gigabyte:N0}gb"
                : $"{bytes / Terabyte:N0}tb";

        /// <summary>
        /// Gets apparent media type information based on a file extension.
        /// </summary>
        /// <param name="extension">The extension.</param>
        /// <returns>Media type information.</returns>
        public static MediaTypeInfo GetMediaTypeInfo(this string extension)
        {
            extension = $".{extension?.ToLower().TrimStart('.')}";
            return ArchiveMimes.ContainsKey(extension) ? new MediaTypeInfo(MediaTypes.Archive, ArchiveMimes[extension])
                : ImageMimes.ContainsKey(extension) ? new MediaTypeInfo(MediaTypes.Image, ImageMimes[extension])
                : AudioMimes.ContainsKey(extension) ? new MediaTypeInfo(MediaTypes.Audio, AudioMimes[extension])
                : VideoMimes.ContainsKey(extension) ? new MediaTypeInfo(MediaTypes.Video, VideoMimes[extension])
                : new MediaTypeInfo(MediaTypes.NonMedia, null);
        }

        /// <summary>
        /// Gets a set of supported extensions for a given media type.
        /// </summary>
        /// <param name="mediaType">The media type.</param>
        /// <returns>A set of file extensions.</returns>
        public static HashSet<string> GetExtensions(this MediaTypes mediaType)
        {
            var extensions = new List<string>();
            if (mediaType.HasFlag(MediaTypes.Archive)) extensions.AddRange(ArchiveMimes.Keys);
            if (mediaType.HasFlag(MediaTypes.Image)) extensions.AddRange(ImageMimes.Keys);
            if (mediaType.HasFlag(MediaTypes.Audio)) extensions.AddRange(AudioMimes.Keys);
            if (mediaType.HasFlag(MediaTypes.Video)) extensions.AddRange(VideoMimes.Keys);
            return new HashSet<string>(extensions, StringComparer.OrdinalIgnoreCase);
        }
    }
}
