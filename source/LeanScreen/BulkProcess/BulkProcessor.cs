// <copyright file="BulkProcessor.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace LeanScreen.BulkProcess;

using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CryptoStream.Encoding;
using CryptoStream.IO;
using CryptoStream.Transform;
using LeanScreen.Common;
using LeanScreen.MediaRepo;
using LeanScreen.Services;

/// <inheritdoc cref="IBulkProcessor"/>
public class BulkProcessor(ISnapService snapper, IMediaRepo repo) : IBulkProcessor
{
    private const string RegexSuffix = @"\.[0-9a-f]+\.[0-9a-f]{10}$";
    private static readonly AesGcmEncryptor Encryptor = new();

    /// <inheritdoc/>
    public async Task<BulkResponse> IngestAsync(
        byte[] key,
        DirectoryInfo source,
        bool recurse,
        bool ingestNonMedia,
        bool purgeNonMatching,
        bool applySnap,
        IProgress<double>? onProgress = null)
    {
        var sourceFiles = source.EnumerateMedia(
            MediaTypes.NonMedia | MediaTypes.AnyMedia,
            recurse: recurse).ToList();
        var retVal = new BulkResponse(sourceFiles.Count);

        onProgress?.Report(0);
        foreach (var file in sourceFiles)
        {
            var typeInfo = file.GetMediaTypeInfo();
            var isSecure = file.IsSecure();
            var pertinent = MediaTypes.AnyMedia.HasFlag(typeInfo.MediaType) || ingestNonMedia;
            if (!pertinent)
            {
                retVal.Unmatched++;
                if (purgeNonMatching)
                {
                    file.Delete();
                }
            }
            else
            {
                byte[] salt;
                if (isSecure)
                {
                    salt = file.ToSalt();
                }
                else
                {
                    using var saltStr = file.OpenRead();
                    salt = Encryptor.GenerateSalt(saltStr, key);
                }

                var saltText = salt.Encode(Codec.ByteHex).ToLowerInvariant();
                var ext = isSecure ? file.Extension : new FileInfo(saltText).ToSecureExtension(file.Extension);
                var itemId = saltText + ext;
                var relatedMedia = await repo.FindAsync(itemId);
                var processed = false;
                if (!relatedMedia.Contains(itemId))
                {
                    if (!isSecure)
                    {
                        file.EncryptInSitu(key, Encryptor);
                        isSecure = true;
                    }

                    using var mediaStr = file.OpenRead();
                    await repo.AddMedia(mediaStr, file.Name);
                    processed = true;
                }

                if (applySnap)
                {
                    var isVideo = file.GetMediaTypeInfo().MediaType == MediaTypes.Video;
                    var rgx = new Regex(itemId.Substring(0, 12) + RegexSuffix);
                    var hasCaps = relatedMedia.Exists(rgx.IsMatch);
                    if (isVideo && !hasCaps)
                    {
                        using var str = file.OpenRead();
                        using var capStream = snapper.Collate(str, isSecure ? salt : [], key, out _, 24, 4, 300);
                        var capSalt = capStream.Encrypt(key);
                        var capExt = new FileInfo(capSalt).ToSecureExtension(".jpg");
                        var capItemId = itemId.Substring(0, 12) + "." + capSalt + capExt;
                        await repo.AddCaps(capStream, capItemId, itemId);
                        processed = true;
                    }
                }

                file.Delete();
                if (processed)
                {
                    retVal.Processed++;
                }
                else
                {
                    retVal.Skipped++;
                }
            }

            onProgress?.Report(retVal.Percent);
        }

        onProgress?.Report(100);
        return retVal;
    }

    /// <inheritdoc/>
    public async Task<int> EnsureCapped(byte[] key, int max = 100, IProgress<double>? onProgress = null)
    {
        var capped = 0;
        var todo = await repo.NextUncapped(max);
        onProgress?.Report(0);
        foreach (var parentId in todo)
        {
            var salt = new FileInfo(parentId).ToSalt();
            using var vidStream = await repo.OpenAsync(parentId);
            using var capStream = snapper.Collate(vidStream, salt, key, out _, 24, 4, 300);
            var capSalt = capStream.Encrypt(key);
            var capExt = new FileInfo(capSalt).ToSecureExtension(".jpg");
            var capItemId = parentId.Substring(0, 12) + "." + capSalt + capExt;
            await repo.AddCaps(capStream, capItemId, parentId);
            capped++;
            onProgress?.Report(100.0 * capped / todo.Count);
        }

        onProgress?.Report(100);
        return capped;
    }
}
