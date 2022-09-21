using System.Collections.Generic;

namespace Av.Models
{
    /// <summary>
    /// Media type information.
    /// </summary>
    public struct MediaTypeInfo
    {
        private static readonly MediaTypeInfo UnmatchedType = new MediaTypeInfo(MediaTypes.NonMedia, null);
        private static readonly IReadOnlyDictionary<string, MediaTypeInfo> Map =
            new Dictionary<string, MediaTypeInfo>
            {
                { ".7z", new MediaTypeInfo(MediaTypes.Archive, "application/x-7z-compressed") },
                { ".gz", new MediaTypeInfo(MediaTypes.Archive, "application/gzip") },
                { ".jar", new MediaTypeInfo(MediaTypes.Archive, "application/java-archive") },
                { ".rar", new MediaTypeInfo(MediaTypes.Archive, "application/vnd.rar") },
                { ".tar", new MediaTypeInfo(MediaTypes.Archive, "application/x-tar") },
                { ".zip", new MediaTypeInfo(MediaTypes.Archive, "application/zip") },
                { ".bmp", new MediaTypeInfo(MediaTypes.Image, "image/bmp") },
                { ".gif", new MediaTypeInfo(MediaTypes.Image, "image/gif") },
                { ".jpe", new MediaTypeInfo(MediaTypes.Image, "image/jpeg") },
                { ".jpeg", new MediaTypeInfo(MediaTypes.Image, "image/jpeg") },
                { ".jpg", new MediaTypeInfo(MediaTypes.Image, "image/jpeg") },
                { ".png", new MediaTypeInfo(MediaTypes.Image, "image/png") },
                { ".tif", new MediaTypeInfo(MediaTypes.Image, "image/tiff") },
                { ".tiff", new MediaTypeInfo(MediaTypes.Image, "image/tiff") },
                { ".webp", new MediaTypeInfo(MediaTypes.Image, "image/webp") },
                { ".aac", new MediaTypeInfo(MediaTypes.Audio, "audio/aac") },
                { ".m4a", new MediaTypeInfo(MediaTypes.Audio, "audio/m4a") },
                { ".mp3", new MediaTypeInfo(MediaTypes.Audio, "audio/mpeg") },
                { ".oga", new MediaTypeInfo(MediaTypes.Audio, "audio/ogg") },
                { ".wav", new MediaTypeInfo(MediaTypes.Audio, "audio/wav") },
                { ".weba", new MediaTypeInfo(MediaTypes.Audio, "audio/webm") },
                { ".3g2", new MediaTypeInfo(MediaTypes.Video, "video/3gpp2") },
                { ".3gp", new MediaTypeInfo(MediaTypes.Video, "video/3gpp") },
                { ".avi", new MediaTypeInfo(MediaTypes.Video, "video/x-msvideo") },
                { ".flv", new MediaTypeInfo(MediaTypes.Video, "video/x-flv") },
                { ".m2v", new MediaTypeInfo(MediaTypes.Video, "video/mpeg") },
                { ".m4v", new MediaTypeInfo(MediaTypes.Video, "video/x-m4v") },
                { ".mkv", new MediaTypeInfo(MediaTypes.Video, "video/x-matroska") },
                { ".mov", new MediaTypeInfo(MediaTypes.Video, "video/quicktime") },
                { ".mp4", new MediaTypeInfo(MediaTypes.Video, "video/mp4") },
                { ".mpeg", new MediaTypeInfo(MediaTypes.Video, "video/mpeg") },
                { ".mpg", new MediaTypeInfo(MediaTypes.Video, "video/mpeg") },
                { ".mts", new MediaTypeInfo(MediaTypes.Video, "video/mp2t") },
                { ".swf", new MediaTypeInfo(MediaTypes.Video, "application/x-shockwave-flash") },
                { ".ts", new MediaTypeInfo(MediaTypes.Video, "video/mp2t") },
                { ".ogg", new MediaTypeInfo(MediaTypes.Video, "video/ogg") },
                { ".ogv", new MediaTypeInfo(MediaTypes.Video, "video/ogg") },
                { ".vob", new MediaTypeInfo(MediaTypes.Video, "video/x-ms-vob") },
                { ".webm", new MediaTypeInfo(MediaTypes.Video, "video/webm") },
                { ".wmv", new MediaTypeInfo(MediaTypes.Video, "video/x-ms-wmv") },
            };

        /// <summary>
        /// Initialises a new <see cref="MediaTypeInfo"/>.
        /// </summary>
        /// <param name="mediaType">The media type.</param>
        /// <param name="mimeType">The mime type.</param>
        public MediaTypeInfo(MediaTypes mediaType, string mimeType)
        {
            this.MediaType = mediaType;
            this.MimeType = mimeType;
        }

        /// <summary>
        /// Gets apparent media type information for a file extension.
        /// </summary>
        /// <remarks>Insensitive to case and any leading dot.</remarks>
        /// <param name="extension">The file extension.</param>
        /// <returns>Media type information or <see langword="null"/>, if the
        /// extension is not recognised.</returns>
        public static MediaTypeInfo Get(string extension)
        {
            extension = $".{extension?.ToLower().TrimStart('.')}";
            return Map.ContainsKey(extension)
                ? Map[extension]
                : UnmatchedType;
        }

        /// <summary>
        /// Gets the media type.
        /// </summary>
        public MediaTypes MediaType { get; }

        /// <summary>
        /// Gets the mime type.
        /// </summary>
        public string MimeType { get; }
    }
}
