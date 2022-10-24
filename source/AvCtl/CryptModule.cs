// <copyright file="CryptModule.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace AvCtl;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Comanche;

/// <summary>
/// Crypt module.
/// </summary>
[Alias("crypt")]
public static class CryptModule
{
    /// <summary>
    /// Encrypts all hitherto-unencrypted media files in source.
    /// </summary>
    /// <param name="source">The source directory.</param>
    /// <param name="keyCsv">The encryption key.</param>
    /// <param name="destination">The output folder.</param>
    [Alias("bulk")]
    public static void EncryptMedia(
        [Alias("s")] string source,
        [Alias("k")] string keyCsv,
        [Alias("d")] string? destination = null)
    {
        // TODO!
    }
}
