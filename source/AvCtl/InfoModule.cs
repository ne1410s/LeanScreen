// <copyright file="InfoModule.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace AvCtl;

using System.Text.Json;
using Comanche.Attributes;

/// <summary>
/// Info module.
/// </summary>
[Module("info")]
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
        var renderer = CommonUtils.GetRenderer(source, keyCsv, out _);
        return JsonSerializer.Serialize(renderer.Media, new JsonSerializerOptions
        {
            WriteIndented = true,
        });
    }
}
