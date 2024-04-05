﻿// <copyright file="BulkProcessor.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace Av.BulkProcess;

using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Av.Common;
using Av.MediaRepo;
using Av.Services;
using Av.Store;
using Crypt.Encoding;
using Crypt.IO;
using Crypt.Transform;

/// <inheritdoc cref="IBulkProcessor"/>
public class BulkProcessor(ISnapService snapper, IMediaRepo repo) : IBulkProcessor
{
    private static readonly AesGcmEncryptor Encryptor = new();

    /// <inheritdoc/>
    public async Task<BulkItemResponse> IngestAsync(
        byte[] key,
        DirectoryInfo source,
        bool recurse,
        bool purgeNonMedia)
    {
        var sourceFiles = source.EnumerateMedia(
            MediaTypes.NonMedia | MediaTypes.AnyMedia,
            recurse: recurse);
        var retVal = new BulkItemResponse { Total = sourceFiles.Count() };

        foreach (var file in sourceFiles)
        {
            var typeInfo = file.GetMediaTypeInfo();
            var isSecure = file.IsSecure();
            var pertinent = MediaTypes.AnyMedia.HasFlag(typeInfo.MediaType);
            if (!pertinent)
            {
                retVal.Unmatched++;
                if (purgeNonMedia)
                {
                    file.Delete();
                }

                continue;
            }

            byte[] salt;
            if (isSecure)
            {
                salt = file.ToSalt();
            }
            else
            {
                using var saltStr = file.OpenRead();
                salt = Encryptor.GenerateSalt(saltStr);
            }

            var itemId = salt.Encode(Codec.ByteHex).ToLower() + file.Extension;
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

            var isVideo = file.GetMediaTypeInfo().MediaType == MediaTypes.Video;
            var hasCaps = relatedMedia.Exists(m => m.StartsWith(itemId.Substring(0, 12)) && m.EndsWith(".jpg"));
            if (isVideo && !hasCaps)
            {
                using var str = file.OpenRead();
                using var capStream = snapper.Collate(str, isSecure ? salt : [], key, 24, 4, 300);
                var capSalt = capStream.Encrypt(key);
                var capItemId = itemId.Substring(0, 12) + "." + capSalt + ".jpg";
                await repo.AddCaps(capStream, capItemId, itemId);
                processed = true;
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

        return retVal;
    }

    /// <inheritdoc/>
    public async Task<int> EnsureCapped(byte[] key, int max = 100)
    {
        var capped = 0;
        var todo = await repo.NextUncapped(max);
        foreach (var parentId in todo)
        {
            var salt = new FileInfo(parentId).ToSalt();
            using var vidStream = await repo.OpenAsync(parentId);
            using var capStream = snapper.Snap(vidStream, salt, key, 0.4, 300);
            var capSalt = capStream.Encrypt(key);
            var capItemId = parentId.Substring(0, 12) + "." + capSalt + ".jpg";
            await repo.AddCaps(capStream, capItemId, parentId);
            capped++;
        }

        return capped;
    }
}