// <copyright file="MediaTypeInfo.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace Av.Models
{
    /// <summary>
    /// Media type information.
    /// </summary>
    public struct MediaTypeInfo
    {
        /// <summary>
        /// Initialises a new instance of the <see cref="MediaTypeInfo"/> struct.
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
