namespace Av.Models
{
    /// <summary>
    /// Media type information.
    /// </summary>
    public struct MediaTypeInfo
    {
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
        /// Gets the media type.
        /// </summary>
        public MediaTypes MediaType { get; }

        /// <summary>
        /// Gets the mime type.
        /// </summary>
        public string MimeType { get; }
    }
}
