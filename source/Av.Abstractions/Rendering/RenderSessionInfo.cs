// <copyright file="RenderSessionInfo.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace Av.Abstractions.Rendering
{
    using System;
    using Av.Abstractions.Shared;

    /// <summary>
    /// Information about the media as part of the open rendering session.
    /// </summary>
    public class RenderSessionInfo
    {
        /// <summary>
        /// Initialises a new instance of the <see cref="RenderSessionInfo"/> class.
        /// </summary>
        /// <param name="frameSize">The frame size.</param>
        public RenderSessionInfo(Dimensions2D frameSize)
        {
            this.FrameSize = frameSize;
        }

        /// <summary>
        /// Gets the target frame size.
        /// </summary>
        public Dimensions2D FrameSize { get; }
    }
}
