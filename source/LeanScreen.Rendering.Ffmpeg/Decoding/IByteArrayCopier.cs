// <copyright file="IByteArrayCopier.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace LeanScreen.Rendering.Ffmpeg.Decoding;

using System;

/// <summary>
/// Copies a pointer to a byte array.
/// </summary>
public interface IByteArrayCopier
{
    /// <summary>
    /// Copies a byte array to a pointer.
    /// </summary>
    /// <param name="source">The source buffer.</param>
    /// <param name="target">The target pointer.</param>
    /// <param name="length">The length to copy.</param>
    void Copy(byte[] source, IntPtr target, int length);

    /// <summary>
    /// Copies a pointer to a byte array.
    /// </summary>
    /// <param name="source">The source pointer.</param>
    /// <param name="target">The target buffer.</param>
    /// <param name="length">The length to copy.</param>
    void Copy(IntPtr source, byte[] target, int length);
}
