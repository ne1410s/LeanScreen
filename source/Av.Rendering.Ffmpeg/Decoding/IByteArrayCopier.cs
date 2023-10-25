// <copyright file="IByteArrayCopier.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace Av.Rendering.Ffmpeg.Decoding;

using System;

/// <summary>
/// Copies a pointer to a byte array.
/// </summary>
public interface IByteArrayCopier
{
    /// <summary>
    /// Copies a pointer to a byte array.
    /// </summary>
    /// <param name="target">The target buffer.</param>
    /// <param name="source">The source buffer.</param>
    /// <param name="length">The length to copy.</param>
    void Copy(byte[] target, IntPtr source, int length);
}
