// <copyright file="BulkMediaUtils.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace LeanScreen.Extensions;

using System;
using System.IO;
using System.Threading.Tasks;
using LeanScreen.BulkProcess;
using LeanScreen.Imaging.SixLabors;
using LeanScreen.MediaRepo;
using LeanScreen.MediaRepo.FileSystem;
using LeanScreen.Rendering.Ffmpeg;
using LeanScreen.Snaps;

/// <summary>
/// Bulk media utilities.
/// </summary>
public static class BulkMediaUtils
{
    private static readonly SnapService Snapper = new(
        new FfmpegRendererFactory(),
        new SixLaborsImagingService());

    /// <summary>
    /// Ingests media into a store.
    /// </summary>
    /// <param name="di">The source directory.</param>
    /// <param name="key">The key.</param>
    /// <param name="mediaRepoParam">The media repo parameter.</param>
    /// <param name="mediaRepoName">The media repo name.</param>
    /// <param name="applySnap">Whether to apply snapshot.</param>
    /// <param name="recurse">Whether to look for sources in subdirectories.</param>
    /// <param name="ingestNonMedia">Whether to ingest non-media files.</param>
    /// <param name="purgeNonMatching">Whether to delete non-matching source files.</param>
    /// <param name="onProgress">Progress handler.</param>
    /// <returns>The response.</returns>
    public static async Task<BulkResponse> Ingest(
        this DirectoryInfo di,
        byte[] key,
        string mediaRepoParam,
        string mediaRepoName = "fs",
        bool applySnap = false,
        bool recurse = true,
        bool ingestNonMedia = false,
        bool purgeNonMatching = true,
        IProgress<double>? onProgress = null)
    {
        var processor = new BulkProcessor(Snapper, GetRepo(mediaRepoName, mediaRepoParam));
        return await processor.IngestAsync(key, di, recurse, ingestNonMedia, purgeNonMatching, applySnap, onProgress);
    }

    /// <summary>
    /// Iterates an existing store, ensuring caps are provided.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <param name="mediaRepoParam">The media repo parameter.</param>
    /// <param name="mediaRepoName">The media repo name.</param>
    /// <param name="max">The maximum number of files to process.</param>
    /// <param name="onProgress">Progress handler.</param>
    /// <returns>The number of new caps added.</returns>
    public static async Task<int> ApplyCaps(
        byte[] key,
        string mediaRepoParam,
        string mediaRepoName = "fs",
        int max = 100,
        IProgress<double>? onProgress = null)
    {
        var processor = new BulkProcessor(Snapper, GetRepo(mediaRepoName, mediaRepoParam));
        return await processor.EnsureCapped(key, max, onProgress);
    }

    /// <summary>
    /// Gets a media repo.
    /// </summary>
    /// <param name="type">The type.</param>
    /// <param name="param">The parameter.</param>
    /// <returns>A new repo.</returns>
    public static IMediaRepo GetRepo(string type, string param) => type switch
    {
        _ => new FileStore(param),
    };
}
