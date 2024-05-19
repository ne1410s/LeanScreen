// <copyright file="ByteArrayCopier.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace Av.Rendering.Ffmpeg.Decoding;

using System;
using System.Runtime.InteropServices;

/// <inheritdoc cref="IByteArrayCopier"/>
internal sealed class ByteArrayCopier : IByteArrayCopier
{
    /// <inheritdoc/>
    public void Copy(byte[] target, IntPtr source, int length)
        => Marshal.Copy(target, 0, source, length);
}
