// <copyright file="MediaTypes.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace Av.Models
{
    using System;

    /// <summary>
    /// Media types.
    /// </summary>
    [Flags]
    public enum MediaTypes
    {
        /// <summary>
        /// Media and non-media types.
        /// </summary>
        Anything = 0b0000,

        /// <summary>
        /// Recognised as non-media.
        /// </summary>
        NonMedia = 0b00001,

        /// <summary>
        /// Audio type.
        /// </summary>
        Audio = 0b00010,

        /// <summary>
        /// Image type.
        /// </summary>
        Image = 0b00100,

        /// <summary>
        /// Video type.
        /// </summary>
        Video = 0b01000,

        /// <summary>
        /// Streamable media types.
        /// </summary>
        Streamable = Audio | Video,

        /// <summary>
        /// Visible media types.
        /// </summary>
        Visible = Image | Video,

        /// <summary>
        /// Any media type.
        /// </summary>
        AnyMedia = Audio | Image | Video,

        /// <summary>
        /// Archive type.
        /// </summary>
        Archive = 0b10000,
    }
}
