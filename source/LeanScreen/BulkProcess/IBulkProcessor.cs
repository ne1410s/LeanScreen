// <copyright file="IBulkProcessor.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace LeanScreen.BulkProcess;

using System;
using System.IO;
using System.Threading.Tasks;

/// <summary>
/// Bulk processor.
/// </summary>
public interface IBulkProcessor
{
    /// <summary>
    /// Ingests media into a store.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <param name="source">The source directory.</param>
    /// <param name="recurse">Whether to include subdirectories.</param>
    /// <param name="ingestNonMedia">Whether to ingest non-media files.</param>
    /// <param name="purgeNonMatching">Whether to delete non-matching source files.</param>
    /// <param name="applySnap">Whether to apply a snapshot on ingest.</param>
    /// <param name="onProgress">Progress handler.</param>
    /// <returns>The response.</returns>
    public Task<BulkResponse> IngestAsync(
        byte[] key,
        DirectoryInfo source,
        bool recurse,
        bool ingestNonMedia,
        bool purgeNonMatching,
        bool applySnap,
        IProgress<double>? onProgress = null);

    /// <summary>
    /// Ensures cappable media in the store is capped.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <param name="max">The maximum number of items to take.</param>
    /// <param name="onProgress">Progress handler.</param>
    /// <returns>Number of newly-capped items.</returns>
    public Task<int> EnsureCapped(
        byte[] key,
        int max = 100,
        IProgress<double>? onProgress = null);
}
