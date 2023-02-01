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
    /// <param name="source">The source file.</param>
    /// <param name="keySource">The key source directory.</param>
    /// <param name="keyRegex">The key source regular expression.</param>
    /// <param name="writer">Output writer.</param>
    /// <returns>Basic information about the source.</returns>
    [Alias("basic")]
    public static string GetBasicInfo(
        [Alias("s")] string source,
        [Alias("ks")] string keySource,
        [Alias("kr")] string keyRegex,
        IOutputWriter? writer = null)
    {
        writer ??= new ConsoleWriter();
        var blendedInput = writer.CaptureStrings().Blend();
        var hashes = CommonUtils.GetHashes(keySource, keyRegex);
        var key = new DefaultKeyDeriver().DeriveKey(blendedInput, hashes);

        var renderer = CommonUtils.GetRenderer(source, key);
        return JsonSerializer.Serialize(renderer.Media, new JsonSerializerOptions
        {
            WriteIndented = true,
        });
    }
}
