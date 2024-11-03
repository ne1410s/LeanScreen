// <copyright file="ByteArrayCopier.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace LeanScreen.Rendering.Ffmpeg.Decoding;

using System;
using System.Runtime.InteropServices;

/// <inheritdoc cref="IByteArrayCopier"/>
internal sealed class ByteArrayCopier : IByteArrayCopier
{
    /// <inheritdoc/>
    public void Copy(byte[] source, IntPtr target, int length)
        => Marshal.Copy(source, 0, target, length);

    /// <inheritdoc/>
    public void Copy(IntPtr source, byte[] target, int length)
        => Marshal.Copy(source, target, 0, length);
}
