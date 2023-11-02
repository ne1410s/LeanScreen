// <copyright file="CryptModule.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace AvCtl;

using Av;
using Av.Models;
using Comanche.Attributes;
using Comanche.Services;
using Crypt.Encoding;
using Crypt.Hashing;
using Crypt.IO;
using Crypt.Keying;

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
    /// <param name="writer">Output writer.</param>
    /// <param name="source">The source directory.</param>
    /// <param name="keySource">The key source directory.</param>
    /// <param name="keyRegex">The key source regular expression.</param>
    /// <param name="groupLabelLength">The grouping label length.</param>
    /// <param name="recurse">Whether to recurse.</param>
    [Alias("bulk")]
    public static void EncryptMedia(
        [Hidden] IOutputWriter writer,
        [Alias("s")] string source,
        [Alias("ks")] string? keySource = null,
        [Alias("kr")] string? keyRegex = null,
        [Alias("g")] int groupLabelLength = 2,
        [Alias("r")] bool recurse = true)
    {
        _ = writer ?? throw new ArgumentNullException(nameof(writer));
        var di = new DirectoryInfo(source);
        var items = di.EnumerateMedia(MediaTypes.AnyMedia, false, recurse, take: int.MaxValue);
        var total = items.Count();
        var done = 0;

        var blendedInput = writer.CaptureStrings().Blend();
        var hashes = CommonUtils.GetHashes(keySource, keyRegex);
        var key = new DefaultKeyDeriver().DeriveKey(blendedInput, hashes);
        var md5Base64 = key.Hash(HashType.Md5).Encode(Codec.ByteBase64);

        writer.WriteLine($"Keys: {hashes.Length}, Check: {md5Base64}");
        writer.WriteLine($"Encryption: Start - Files: {total}");
        foreach (var item in items)
        {
            item.EncryptInSitu(key);
            item.GroupByLabel(di, groupLabelLength);
            writer.WriteLine($"Done: {++done * 100.0 / total:N2}%");
        }

        writer.WriteLine("Encryption: End");
        foreach (var notDone in di.EnumerateMedia(MediaTypes.NonMedia, false, recurse, take: int.MaxValue))
        {
            writer.WriteLine($" - Not secured: {notDone.FullName}");
        }
    }

    /// <summary>
    /// Encrypts a single file. The file is secured "in situ"; overwriting the
    /// original bytes with new ones.
    /// </summary>
    /// <param name="writer">Output writer.</param>
    /// <param name="source">The source file.</param>
    /// <param name="keySource">The key source directory.</param>
    /// <param name="keyRegex">The key source regular expression.</param>
    /// <returns>The encrypted file name.</returns>
    [Alias("file")]
    public static string EncryptFile(
        [Hidden] IOutputWriter writer,
        [Alias("s")] string source,
        [Alias("ks")] string? keySource = null,
        [Alias("kr")] string? keyRegex = null)
    {
        _ = writer ?? throw new ArgumentNullException(nameof(writer));
        var fi = new FileInfo(source);

        var blendedInput = writer.CaptureStrings().Blend();
        var hashes = CommonUtils.GetHashes(keySource, keyRegex);
        var key = new DefaultKeyDeriver().DeriveKey(blendedInput, hashes);
        var md5Base64 = key.Hash(HashType.Md5).Encode(Codec.ByteBase64);

        writer.WriteLine($"Keys: {hashes.Length}, Check: {md5Base64}");
        fi.EncryptInSitu(key);

        return fi.FullName;
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
