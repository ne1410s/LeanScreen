﻿// <copyright file="FileExtensions.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace Av
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Av.Models;
    using Crypt.IO;

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
            => (fi ?? throw new ArgumentNullException(nameof(fi))).Extension.GetMediaTypeInfo();

        /// <summary>
        /// Enumerates all media files recursively, according to the specified
        /// media type(s).
        /// </summary>
        /// <param name="di">The root directory info.</param>
        /// <param name="mediaTypes">The media type(s) to look for.</param>
        /// <param name="secure">Whether to look only for secure or non-secure
        /// items. If null, both these states are included.</param>
        /// <param name="recurse">Whether to recurse.</param>
        /// <param name="skip">Files to skip.</param>
        /// <param name="take">Files to take.</param>
        /// <returns>A sequence of file paths.</returns>
        public static IEnumerable<FileInfo> EnumerateMedia(
            this DirectoryInfo di,
            MediaTypes mediaTypes,
            bool? secure = null,
            bool recurse = false,
            int skip = 0,
            int take = int.MaxValue)
        {
            di = di ?? throw new ArgumentNullException(nameof(di));
            var searchOpt = recurse ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
            return di
                .EnumerateFiles(AllFilesWildcard, searchOpt)
                .Where(fi => (secure == null || fi.IsSecure() == secure)
                    && mediaTypes.HasFlag(fi.GetMediaTypeInfo().MediaType))
                .Skip(skip)
                .Take(take);
        }
    }
}
