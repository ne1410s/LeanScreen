// <copyright file="ISnapService.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace LeanScreen.Services;

using System.IO;
using LeanScreen.Common;
using LeanScreen.Rendering;

/// <summary>
/// Snap service.
/// </summary>
public interface ISnapService
{
    /// <summary>
    /// Gets media info.
    /// </summary>
    /// <param name="stream">The input stream.</param>
    /// <param name="salt">The salt.</param>
    /// <param name="key">The key.</param>
    /// <returns>Media info.</returns>
    public MediaInfo GetInfo(
        Stream stream, byte[] salt, byte[] key);

    /// <summary>
    /// Snaps a single frame.
    /// </summary>
    /// <param name="stream">The input stream.</param>
    /// <param name="salt">The salt.</param>
    /// <param name="key">The key.</param>
    /// <param name="size">The output frame dimensions.</param>
    /// <param name="position">The requested position.</param>
    /// <param name="height">The requested item height.</param>
    /// <returns>Unencrypted image stream.</returns>
    public MemoryStream Snap(
        Stream stream, byte[] salt, byte[] key, out Size2D size, double position = 0.4, int? height = 300);

    /// <summary>
    /// Collates a sequence of frames.
    /// </summary>
    /// <param name="stream">The input stream.</param>
    /// <param name="salt">The salt.</param>
    /// <param name="key">The key.</param>
    /// <param name="size">The output frame dimensions.</param>
    /// <param name="total">The total number of items.</param>
    /// <param name="columns">The maximum number of columns.</param>
    /// <param name="height">The requested item height.</param>
    /// <param name="border">Whether to include item borders.</param>
    /// <returns>Unencrypted image stream.</returns>
    public MemoryStream Collate(
        Stream stream,
        byte[] salt,
        byte[] key,
        out Size2D size,
        int total = 24,
        int columns = 4,
        int? height = 300,
        bool border = true);
}
