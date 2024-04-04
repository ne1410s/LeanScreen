// <copyright file="BulkItemResponse.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace Av.Store;

/// <summary>
/// Bulk item response.
/// </summary>
public record BulkItemResponse
{
    /// <summary>
    /// Gets or sets the total number of files.
    /// </summary>
    public int Total { get; set; }

    /// <summary>
    /// Gets or sets the number of unmatched files.
    /// </summary>
    public int Unmatched { get; set; }

    /// <summary>
    /// Gets or sets the number of skipped files.
    /// </summary>
    public int Skipped { get; set; }

    /// <summary>
    /// Gets or sets the number of processed files.
    /// </summary>
    public int Processed { get; set; }
}
