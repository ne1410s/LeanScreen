// <copyright file="FileStore.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace Av.Store.FileSystem;

using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Av.Common;
using Av.MediaRepo;
using Crypt.IO;

/// <inheritdoc cref="IMediaRepo"/>
public class FileStore : IMediaRepo
{
    private const int GroupLength = 2;
    private readonly DirectoryInfo di;

    /// <summary>
    /// Initializes a new instance of the <see cref="FileStore"/> class.
    /// </summary>
    /// <param name="dir">The directory path.</param>
    public FileStore(string dir)
    {
        this.di = new DirectoryInfo(dir);
    }

    /// <inheritdoc/>
    public Task<List<string>> FindAsync(string itemId)
    {
        var pattern = itemId.NotNull().Substring(0, 12) + "*";
        var files = this.di.GetFiles(pattern, SearchOption.AllDirectories);
        return Task.FromResult(files.Select(r => r.Name).ToList());
    }

    /// <inheritdoc/>
    public Task<List<string>> NextUncapped(int max = 100)
    {
        var retVal = new List<string>();
        foreach (var fi in this.di.EnumerateMedia(MediaTypes.Video, true, true))
        {
            var counterpart = fi.Directory!.GetFiles(fi.Name.Substring(0, 12)).Length > 1;
            if (!counterpart)
            {
                retVal.Add(fi.Name);
                if (retVal.Count >= max)
                {
                    break;
                }
            }
        }

        return Task.FromResult(retVal);
    }

    /// <inheritdoc/>
    public async Task AddMedia(Stream source, string itemId)
    {
        source = source.NotNull();
        itemId = itemId.NotNull();
        if (!new FileInfo(itemId).IsSecure())
        {
            throw new InvalidOperationException("Source is not secure");
        }

        var matches = await this.FindAsync(itemId);
        if (!matches.Exists(m => m == itemId))
        {
            var folderName = itemId.Substring(0, GroupLength);
            var folder = this.di.CreateSubdirectory(folderName);

            var targetPath = Path.Combine(folder.FullName, itemId);
            using var ss = File.OpenWrite(targetPath);
            source.CopyTo(ss);
        }
    }

    /// <inheritdoc/>
    public async Task AddCaps(Stream source, string itemId, string parentId)
    {
        if (!itemId.NotNull().StartsWith(parentId.NotNull().Substring(0, 12)))
        {
            throw new InvalidOperationException("Source name is not consistent with parent.");
        }

        await this.AddMedia(source, itemId);
    }

    /// <inheritdoc/>
    public Task<Stream> OpenAsync(string itemId)
    {
        itemId = itemId.NotNull();
        var folderName = itemId.Substring(0, GroupLength);
        var folder = this.di.CreateSubdirectory(folderName);
        var targetPath = Path.Combine(folder.FullName, itemId);
        return Task.FromResult<Stream>(File.OpenRead(targetPath));
    }
}
