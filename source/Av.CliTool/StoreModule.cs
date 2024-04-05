// <copyright file="StoreModule.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace Av.CliTool;

using Av;
using Av.Extensions;
using Av.Store;
using Comanche.Attributes;
using Comanche.Services;

/// <summary>
/// The store module.
/// </summary>
[Module("store")]
public static class StoreModule
{
    /// <summary>
    /// Finds media on disk and ingests it into a store.
    /// </summary>
    /// <param name="writer">Output writer.</param>
    /// <param name="source">The source directory.</param>
    /// <param name="storeParam">The store parameter.</param>
    /// <param name="storeType">The target store type. Can be: fs | blob.</param>
    /// <param name="keySource">The key source directory.</param>
    /// <param name="keyRegex">The key source regular expression.</param>
    /// <param name="recurse">Whether to recurse.</param>
    /// <param name="purge">Whether to delete non-pertinent files.</param>
    /// <returns>Process summary.</returns>
    public static async Task<BulkResponse> Ingest(
        [Hidden] IOutputWriter writer,
        [Alias("s")] string source,
        [Alias("sp")] string storeParam,
        [Alias("st")] string storeType = "fs",
        [Alias("ks")] string? keySource = null,
        [Alias("kr")] string? keyRegex = null,
        [Alias("r")] bool recurse = true,
        [Alias("p")] bool purge = false)
    {
        writer = writer.NotNull();
        var di = new DirectoryInfo(source);
        var key = writer.PrepareKey(keySource, keyRegex);
        var result = await di.Ingest(key, storeParam, storeType, recurse, purge, writer.ProgressHandler());
        await Task.Delay(1000);
        writer.Write(line: true);
        return result;
    }

    /// <summary>
    /// Finds uncapped video in a store and caps it.
    /// </summary>
    /// <param name="writer">Output writer.</param>
    /// <param name="storeParam">The store parameter.</param>
    /// <param name="storeType">The target store type. Can be: fs | blob.</param>
    /// <param name="max">The maximum number of items to process.</param>
    /// <param name="keySource">The key source directory.</param>
    /// <param name="keyRegex">The key source regular expression.</param>
    /// <returns>The number of new caps.</returns>
    public static async Task<int> AddCaps(
        [Hidden] IOutputWriter writer,
        [Alias("sp")] string storeParam,
        [Alias("st")] string storeType = "fs",
        [Alias("m")] int max = 100,
        [Alias("ks")] string? keySource = null,
        [Alias("kr")] string? keyRegex = null)
    {
        writer = writer.NotNull();
        var key = writer.PrepareKey(keySource, keyRegex);
        var result = await BulkMediaUtils.ApplyCaps(key, storeParam, storeType, max, writer.ProgressHandler());
        await Task.Delay(1000);
        writer.Write(line: true);
        return result;
    }
}
