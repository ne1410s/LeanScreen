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
    /// Implementation for collation services.
    /// </summary>
    public interface ICollatingService
    {
        /// <summary>
        /// Collates a sequence of frames.
        /// </summary>
        /// <param name="frames">The frames.</param>
        /// <param name="opts">Collation options.</param>
        /// <returns>A stream of encoded image bytes.</returns>
        MemoryStream Collate(IEnumerable<RenderedFrame> frames, CollationOptions opts = null);
    }
}
