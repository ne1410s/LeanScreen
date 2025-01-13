// <copyright file="StoreModule.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace LeanScreen.CliTool;

using Comanche;
using LeanScreen.BulkProcess;
using LeanScreen.Extensions;

/// <summary>
/// The store module.
/// </summary>
public class StoreModule(IConsole console) : IModule
{
    /// <summary>
    /// Finds files on disk and ingests it into a store.
    /// </summary>
    /// <param name="source">The source directory.</param>
    /// <param name="storeParam">The store parameter.</param>
    /// <param name="storeType">The target store type. Can be: fs | blob.</param>
    /// <param name="applySnap">Whether to apply snapshots.</param>
    /// <param name="keySource">The key source directory.</param>
    /// <param name="keyRegex">The key source regular expression.</param>
    /// <param name="recurse">Whether to recurse.</param>
    /// <param name="nonMedia">Whether to include non-media files.</param>
    /// <param name="purge">Whether to delete non-pertinent files.</param>
    /// <returns>Process summary.</returns>
    public async Task<BulkResponse> Ingest(
        [Alias("s")] string source,
        [Alias("sp")] string storeParam,
        [Alias("st")] string storeType = "fs",
        [Alias("as")] bool applySnap = false,
        [Alias("ks")] string? keySource = null,
        [Alias("kr")] string? keyRegex = null,
        [Alias("r")] bool recurse = true,
        [Alias("nm")] bool nonMedia = false,
        [Alias("p")] bool purge = false)
    {
        var di = new DirectoryInfo(source);
        var key = console.PrepareKey(keySource, keyRegex);
        var result = await di.Ingest(
            key, storeParam, storeType, applySnap, recurse, nonMedia, purge, console.ProgressHandler());
        await Task.Delay(1000);
        console.WriteLine();
        return result;
    }

    /// <summary>
    /// Finds uncapped video in a store and caps it.
    /// </summary>
    /// <param name="storeParam">The store parameter.</param>
    /// <param name="storeType">The target store type. Can be: fs | blob.</param>
    /// <param name="max">The maximum number of items to process.</param>
    /// <param name="keySource">The key source directory.</param>
    /// <param name="keyRegex">The key source regular expression.</param>
    /// <returns>The number of new caps.</returns>
    public async Task<int> AddCaps(
        [Alias("sp")] string storeParam,
        [Alias("st")] string storeType = "fs",
        [Alias("m")] int max = 100,
        [Alias("ks")] string? keySource = null,
        [Alias("kr")] string? keyRegex = null)
    {
        var key = console.PrepareKey(keySource, keyRegex);
        var result = await BulkMediaUtils.ApplyCaps(key, storeParam, storeType, max, console.ProgressHandler());
        await Task.Delay(1000);
        console.WriteLine();
        return result;
    }
}
