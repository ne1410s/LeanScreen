// <copyright file="FolderExtensions.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace Av.Extensions;

using System.IO;
using System.Threading.Tasks;
using Av.BulkProcess;
using Av.Imaging.SixLabors;
using Av.MediaRepo;
using Av.Rendering.Ffmpeg;
using Av.Snaps;
using Av.Store;
using Av.Store.AzureBlob;
using Av.Store.FileSystem;

/// <summary>
/// Extensions for folders.
/// </summary>
public static class FolderExtensions
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
    /// <param name="recurse">Whether to look for sources in subdirectories.</param>
    /// <param name="purgeNonMedia">Whether to delete non-media source.</param>
    /// <returns>The response.</returns>
    public static async Task<BulkItemResponse> Ingest(
        this DirectoryInfo di,
        byte[] key,
        string mediaRepoParam,
        string mediaRepoName = "fs",
        bool recurse = true,
        bool purgeNonMedia = true)
    {
        var processor = new BulkProcessor(Snapper, GetRepo(mediaRepoName, mediaRepoParam));
        return await processor.IngestAsync(key, di, recurse, purgeNonMedia);
    }

    private static IMediaRepo GetRepo(string type, string param) => type switch
    {
        "blob" => new AzBlobStore(param),
        _ => new FileStore(param),
    };
}
