// <copyright file="IFormatConverter.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace LeanScreen.Abstractions.Conversion;

using System.IO;
using CryptoStream.Streams;
using LeanScreen.Rendering;

/// <summary>
/// Handles format conversion.
/// </summary>
public interface IFormatConverter
{
    /// <summary>
    /// Remultiplexes media into new format (i.e. lightweight conversion, not wholesale transoding).
    /// </summary>
    /// <param name="source">The source.</param>
    /// <param name="sourceInfo">The source info.</param>
    /// <returns>The output stream.</returns>
    public Stream Remux(ISimpleReadStream source, MediaInfo sourceInfo);
}
