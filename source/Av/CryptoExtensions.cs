// <copyright file="CryptoExtensions.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace Av;

using System.IO;
using Crypt.Encoding;
using Crypt.Transform;

/// <summary>
/// Crypto extensions.
/// </summary>
public static class CryptoExtensions
{
    /// <summary>
    /// Encrypts a stream, obtaining salt hex.
    /// </summary>
    /// <param name="stream">The stream.</param>
    /// <param name="key">The key.</param>
    /// <returns>The salt.</returns>
    public static string Encrypt(this MemoryStream stream, byte[] key)
    {
        var retVal = new AesGcmEncryptor()
            .Encrypt(stream, stream, key)
            .Encode(Codec.ByteHex)
            .ToLowerInvariant();
        stream.NotNull().Seek(0, SeekOrigin.Begin);
        return retVal;
    }
}
