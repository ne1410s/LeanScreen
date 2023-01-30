// <copyright file="CryptModule.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace AvCtl;

using Av;
using Av.Models;
using Comanche.Attributes;
using Comanche.Services;
using Crypt.IO;

/// <summary>
/// Crypt module.
/// </summary>
[Module("crypt")]
public static class CryptModule
{
    /// <summary>
    /// Encrypts all hitherto-unencrypted media files in source. The files are
    /// secured "in situ"; overwriting the original bytes with new ones. Files
    /// are then grouped under the source; to the specified label length.
    /// </summary>
    /// <param name="source">The source directory.</param>
    /// <param name="keyCsv">The encryption key.</param>
    /// <param name="groupLabelLength">The grouping label length.</param>
    /// <param name="writer">Output writer.</param>
    [Alias("bulk")]
    public static void EncryptMedia(
        [Alias("s")] string source,
        [Alias("k")] string keyCsv,
        [Alias("g")] int groupLabelLength = 2,
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
            item.GroupByLabel(di, groupLabelLength);
            writer.WriteLine($"Done: {++done * 100.0 / total:N2}%");
        }

        writer.WriteLine("Encryption: End");
        foreach (var notDone in di.EnumerateMedia(MediaTypes.NonMedia, false))
        {
            writer.WriteLine($" - Not secured: {notDone.FullName}");
        }
    }

    private static void GroupByLabel(
        this FileInfo fi,
        DirectoryInfo root,
        int length)
    {
        var folderName = fi.Name[..length];
        var folder = root.CreateSubdirectory(folderName);
        fi.MoveTo(Path.Combine(folder.FullName, fi.Name), true);
    }
}
