// <copyright file="IMediaRepo.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace Av.MediaRepo;

using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

/// <summary>
/// Media repository.
/// </summary>
public interface IMediaRepo
{
    /// <summary>
    /// Finds all content related to the supplied id.
    /// </summary>
    /// <param name="itemId">The item id.</param>
    /// <returns>A list of matching content references.</returns>
    public Task<List<string>> FindAsync(string itemId);

    /// <summary>
    /// Adds media to the repo.
    /// </summary>
    /// <param name="source">The source.</param>
    /// <param name="itemId">The item id.</param>
    /// <returns>Async task.</returns>
    public Task AddMedia(Stream source, string itemId);

    /// <summary>
    /// Adds caps to a video.
    /// </summary>
    /// <param name="source">The source.</param>
    /// <param name="itemId">The item id.</param>
    /// <param name="parentId">The parent video id.</param>
    /// <returns>Async task.</returns>
    public Task AddCaps(Stream source, string itemId, string parentId);

    /// <summary>
    /// Opens an existing item.
    /// </summary>
    /// <param name="itemId">The item id.</param>
    /// <returns>Stream.</returns>
    public Task<Stream> OpenAsync(string itemId);

    /// <summary>
    /// Gets a list of uncapped video references.
    /// </summary>
    /// <param name="max">The maximum number of videos to return.</param>
    /// <returns>Uncapped video references.</returns>
    public Task<List<string>> NextUncapped(int max = 100);
}
