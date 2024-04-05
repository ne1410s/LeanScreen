// <copyright file="BulkResponse.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace Av.BulkProcess;

/// <summary>
/// Bulk item response.
/// </summary>
/// <param name="Total">Gets the total number of items.</param>
public record BulkResponse(int Total)
{
    /// <summary>
    /// Gets the percentage completion.
    /// </summary>
    public double Percent => 100.0 * (this.Unmatched + this.Skipped + this.Processed) / this.Total;

    /// <summary>
    /// Gets or sets the number of unmatched items.
    /// </summary>
    public int Unmatched { get; set; }

    /// <summary>
    /// Gets or sets the number of skipped items.
    /// </summary>
    public int Skipped { get; set; }

    /// <summary>
    /// Gets or sets the number of processed items.
    /// </summary>
    public int Processed { get; set; }
}
