// <copyright file="InfoModule.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace AvCtl;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Av.Abstractions.Rendering;
using Av.Rendering.Ffmpeg;
using Comanche;

/// <summary>
/// Info module.
/// </summary>
[Alias("info")]
public static class InfoModule
{
    /// <summary>
    /// Generates basic information.
    /// </summary>
    /// <param name="source">The source file.</param>
    /// <param name="keyCsv">Key (if source is encrypted).</param>
    /// <returns>Basic information about the source.</returns>
    [Alias("basic")]
    public static string GetBasicInfo(
        [Alias("s")] string source,
        [Alias("k")] string? keyCsv = null)
    {
        var key = keyCsv?.Split(',').Select(b => byte.Parse(b, CultureInfo.InvariantCulture)).ToArray();
        IRenderingService renderer = new FfmpegRenderer(source, key);

        return "splat";
    }
}
