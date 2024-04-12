// <copyright file="IRenderingSessionFactory.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace Av.Rendering;

using System.IO;

/// <summary>
/// Rendering session factory.
/// </summary>
public interface IRenderingSessionFactory
{
    /// <summary>
    /// Creates a new rendering session.
    /// </summary>
    /// <param name="stream">The source stream.</param>
    /// <param name="salt">The salt.</param>
    /// <param name="key">The key.</param>
    /// <param name="height">Requested item height.</param>
    /// <returns>A rendering session.</returns>
    public IRenderingSession Create(Stream stream, byte[] salt, byte[] key, int? height);
}
