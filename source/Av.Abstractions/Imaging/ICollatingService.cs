// <copyright file="ICollatingService.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace Av.Abstractions.Imaging
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using Av.Abstractions.Rendering;

    /// <summary>
    /// Implementation for collating service.
    /// </summary>
    public interface ICollatingService
    {
        /// <summary>
        /// Gets the frame list. This can be added to, inserted, cleared and
        /// otherwise manipulated prior to rendering. The ordering in this list
        /// is used for collation rendering (unless otherwise specified).
        /// </summary>
        List<RenderedFrame> Frames { get; }

        /// <summary>
        /// Renders the collation.
        /// </summary>
        /// <param name="opts">The collation options.</param>
        /// <returns>An image rendered to memory.</returns>
        MemoryStream RenderCollation(CollationOptions opts = null);
    }
}