﻿// <copyright file="IRenderingService.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace Av.Abstractions.Rendering
{
    using System;
    using Av.Abstractions.Shared;

    /// <summary>
    /// Obtains image stills from a video.
    /// </summary>
    public interface IRenderingService
    {
        /// <summary>
        /// Gets source info.
        /// </summary>
        MediaInfo SourceInfo { get; }

        /// <summary>
        /// Gets render session info.
        /// </summary>
        RenderSessionInfo SessionInfo { get; }

        /// <summary>
        /// Renders a frame at the position specified.
        /// </summary>
        /// <param name="position">The position.</param>
        /// <returns>The rendered frame.</returns>
        RenderedFrame RenderAt(TimeSpan position);
    }
}
