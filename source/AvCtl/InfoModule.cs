// <copyright file="InfoModule.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace AvCtl;

using System.Text.Json;
using Comanche.Attributes;
using Comanche.Services;
using Crypt.Keying;

/// <summary>
/// Info module.
/// </summary>
[Module("info")]
public static class InfoModule
{
    /// <summary>
    /// Generates basic information.
    /// </summary>
    /// <param name="writer">Output writer.</param>
    /// <param name="source">The source file.</param>
    /// <param name="keySource">The key source directory.</param>
    /// <param name="keyRegex">The key source regular expression.</param>
    /// <returns>Basic information about the source.</returns>
    [Alias("basic")]
    public static string GetBasicInfo(
        [Hidden] IOutputWriter writer,
        [Alias("s")] string source,
        [Alias("ks")] string? keySource = null,
        [Alias("kr")] string? keyRegex = null)
    {
        _ = writer ?? throw new ArgumentNullException(nameof(writer));
        var blendedInput = writer.CaptureStrings().Blend();
        var hashes = CommonUtils.GetHashes(keySource, keyRegex);
        var key = new DefaultKeyDeriver().DeriveKey(blendedInput, hashes);

        using var renderer = CommonUtils.GetRenderer();
        renderer.SetSource(source, key);
        return JsonSerializer.Serialize(renderer.Media, new JsonSerializerOptions
        {
            WriteIndented = true,
        });
    }
}
