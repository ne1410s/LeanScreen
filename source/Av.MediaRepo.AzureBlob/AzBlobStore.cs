// <copyright file="AzBlobStore.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace Av.MediaRepo.AzureBlob;

using System.Linq;
using Av.MediaRepo;
using Azure.Storage.Blobs;
using Crypt.IO;

/// <inheritdoc cref="IMediaRepo"/>
[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
public class AzBlobStore : IMediaRepo
{
    private readonly BlobContainerClient container;

    /// <summary>
    /// Initializes a new instance of the <see cref="AzBlobStore"/> class.
    /// </summary>
    /// <param name="connection">Connection string.</param>
    public AzBlobStore(string connection)
    {
        var client = new BlobServiceClient(connection);
        this.container = client.GetBlobContainerClient("test");
        if (!this.container.Exists())
        {
            this.container.Create();
        }
    }

    /// <inheritdoc/>
    public async Task<List<string>> FindAsync(string itemId)
    {
        var retVal = new List<string>();
        var blob = this.container.GetBlobClient(itemId);
        if (await blob.ExistsAsync())
        {
            retVal.Add(blob.Name);
        }

        await foreach (var related in this.container.FindBlobsByTagsAsync($"parent={itemId}"))
        {
            retVal.Add(related.BlobName);
        }

        return retVal;
    }

    /// <inheritdoc/>
    public async Task<List<string>> NextUncapped(int max = 100)
    {
        var pageable = this.container.FindBlobsByTagsAsync("caps=none");
        var retVal = new List<string>();
        await foreach (var page in pageable.AsPages(pageSizeHint: max))
        {
            retVal.AddRange(page.Values.Take(max).Select(v => v.BlobName));
            if (retVal.Count == max)
            {
                break;
            }
        }

        return retVal;
    }

    /// <inheritdoc/>
    public async Task AddMedia(Stream source, string itemId)
    {
        var fi = new FileInfo(itemId);
        if (!fi.IsSecure())
        {
            throw new InvalidOperationException("Source is not secure");
        }

        var matches = await this.FindAsync(itemId);
        if (!matches.Exists(m => m == itemId))
        {
            var blob = this.container.GetBlobClient(itemId);
            var tags = new Dictionary<string, string>();
            if (fi.GetMediaTypeInfo().MediaType == Common.MediaTypes.Video)
            {
                tags.Add("caps", "none");
            }

            await blob.UploadAsync(source, options: new() { Tags = tags });
        }
    }

    /// <inheritdoc/>
    public async Task AddCaps(Stream source, string itemId, string parentId)
    {
        if (!itemId.NotNull().StartsWith(parentId.NotNull().Substring(0, 12)))
        {
            throw new InvalidOperationException("Source name is not consistent with parent.");
        }

        var parentBlob = this.container.GetBlobClient(parentId);
        var parentOgMeta = (await parentBlob.GetPropertiesAsync()).Value.Metadata;

        await this.AddMedia(source, itemId);
        var mainBlob = this.container.GetBlobClient(itemId);
        var mainTags = new Dictionary<string, string> { ["parent"] = parentId, ["caps"] = null! };
        await mainBlob.SetTagsAsync(mainTags);

        parentOgMeta["caps"] = itemId;
        await parentBlob.SetMetadataAsync(parentOgMeta);
    }

    /// <inheritdoc/>
    public Task<Stream> OpenAsync(string itemId)
    {
        itemId = itemId.NotNull();
        var blob = this.container.GetBlobClient(itemId);
        return blob.OpenReadAsync();
    }
}
