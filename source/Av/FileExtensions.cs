// <copyright file="FileExtensions.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace Av
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Av.Models;

    /// <summary>
    /// Extensions for <see cref="FileInfo"/>.
    /// </summary>
    public static class FileExtensions
    {
        private const string AllFilesWildcard = "*";

        /// <summary>
        /// Gets media type information for the file.
        /// </summary>
        /// <param name="fi">The file.</param>
        /// <returns>Media type information.</returns>
        public static MediaTypeInfo GetMediaTypeInfo(this FileInfo fi)
            => fi.Extension.GetMediaTypeInfo();

        /// <summary>
        /// Enumerates all media files recursively, according to the specified
        /// media type(s).
        /// </summary>
        /// <param name="di">The root directory info.</param>
        /// <param name="mediaTypes">The media type(s) to look for.</param>
        /// <returns>A sequence of file paths.</returns>
        public static IEnumerable<FileInfo> EnumerateMedia(
            this DirectoryInfo di,
            MediaTypes mediaTypes) => di
                .EnumerateFiles(AllFilesWildcard, SearchOption.AllDirectories)
                .Where(fi => mediaTypes.HasFlag(fi.GetMediaTypeInfo().MediaType));
    }
}
