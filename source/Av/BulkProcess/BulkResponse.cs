// <copyright file="BulkResponse.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace Av.Store;

/// <summary>
/// Bulk item response.
/// </summary>
public record BulkResponse
{
    /// <summary>
    /// Gets the percentage completion.
    /// </summary>
    public double Percent => 100.0 * (this.Unmatched + this.Skipped + this.Processed) / this.Total;

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
