// <copyright file="IngestModule.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace Av.CliTool;

using Av;
using Av.Extensions;
using Av.Store;
using Comanche.Attributes;
using Comanche.Services;

/// <summary>
/// The ingest module.
/// </summary>
public static class IngestModule
{
    /// <summary>
    /// Finds media on disk and arranges it in a store.
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
    public static async Task<BulkItemResponse> Bulk(
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
        return await di.Ingest(key, storeParam, storeType, recurse, purge);
    }
}
