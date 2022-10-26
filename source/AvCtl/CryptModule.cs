// <copyright file="CryptModule.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace AvCtl;

using Av;
using Av.Models;
using Comanche;
using Comanche.Services;
using Crypt.IO;

/// <summary>
/// Crypt module.
/// </summary>
[Alias("crypt")]
public static class CryptModule
{
    /// <summary>
    /// Encrypts all hitherto-unencrypted media files in source. The files are
    /// secured "in situ"; overwriting the original bytes with new ones.
    /// </summary>
    /// <param name="source">The source directory.</param>
    /// <param name="keyCsv">The encryption key.</param>
    /// <param name="writer">Output writer.</param>
    [Alias("bulk")]
    public static void EncryptMedia(
        [Alias("s")] string source,
        [Alias("k")] string keyCsv,
        IOutputWriter? writer = null)
    {
        var di = new DirectoryInfo(source);
        var key = CommonUtils.GetKey(keyCsv);
        var items = di.EnumerateMedia(MediaTypes.AnyMedia, false);
        writer ??= new ConsoleWriter();
        var total = items.Count();
        var done = 0;

        writer.WriteLine($"Encryption: Start - Files: {total}");
        foreach (var item in items)
        {
            item.EncryptInSitu(key);
            writer.WriteLine($"Done: {++done * 100.0 / total:N2}%");
        }

        writer.WriteLine("Encryption: End");
        foreach (var notDone in di.EnumerateMedia(MediaTypes.NonMedia, false))
        {
            writer.WriteLine($" - Not secured: {notDone.FullName}");
        }
    }
}
