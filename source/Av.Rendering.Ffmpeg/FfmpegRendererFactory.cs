// <copyright file="FfmpegRendererFactory.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace Av.Rendering.Ffmpeg;

using System.IO;

/// <inheritdoc cref="IRenderingSessionFactory"/>
public class FfmpegRendererFactory : IRenderingSessionFactory
{
    /// <inheritdoc />
    public IRenderingSession Create(Stream stream, byte[] salt, byte[] key, int? height)
    {
        return new FfmpegRenderer(stream, salt, key, height);
    }
}
