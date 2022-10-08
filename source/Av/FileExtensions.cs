// <copyright file="FileExtensions.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace Av
{
    using System.IO;
    using Av.Models;

    /// <summary>
    /// Extensions for <see cref="FileInfo"/>.
    /// </summary>
    public static class FileExtensions
    {
        /// <summary>
        /// Gets media type information for the file.
        /// </summary>
        /// <param name="fi">The file.</param>
        /// <returns>Media type information.</returns>
        public static MediaTypeInfo GetMediaTypeInfo(this FileInfo fi)
            => fi.Extension.GetMediaTypeInfo();
    }
}
