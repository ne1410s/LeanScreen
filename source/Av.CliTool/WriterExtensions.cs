// <copyright file="WriterExtensions.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace Av.CliTool;

using System.Text.RegularExpressions;
using Comanche.Models;
using Comanche.Services;

/// <summary>
/// Extensions for the output writer.
/// </summary>
public static class WriterExtensions
{
    /// <summary>
    /// Prepares a key.
    /// </summary>
    /// <param name="writer">The writer.</param>
    /// <param name="keySource">The key source.</param>
    /// <param name="keyRegex">The key regex.</param>
    /// <returns>The key.</returns>
    public static byte[] PrepareKey(this IOutputWriter writer, string? keySource, string? keyRegex)
    {
        writer = writer.NotNull();
        var keyDi = keySource == null ? null : new DirectoryInfo(keySource);
        var keyReg = keyRegex == null ? null : new Regex(keyRegex);
        var key = keyDi.MakeKey(keyReg, writer.CaptureStrings(mask: '*'), out var checkSum);
        writer.Write(checkSum, WriteStyle.Highlight2, line: true);
        writer.CaptureStrings("[Press RETURN to continue]", max: 1);
        return key.ToArray();
    }
}
