// <copyright file="WriterExtensions.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace Av.CliTool;

using System.Text;
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
        writer.Write(line: true);
        return key.ToArray();
    }

    /// <summary>
    /// Gets a progress writer.
    /// </summary>
    /// <param name="writer">The writer.</param>
    /// <returns>Progress handler.</returns>
    public static IProgress<double> ProgressHandler(this IOutputWriter writer)
    {
        const int totalBars = 20;
        var str = new StringBuilder(60);

        Action<double> act = d =>
        {
            //var bars = (int)(d / 100 * totalBars);
            //str.Clear();
            //str.Append(new string('\b', 30));
            //str.Append('|');
            //str.Append(new string('-', bars));
            //str.Append(new string(' ', totalBars - bars));
            //str.Append("| ");
            //str.Append($"{d:N1}".PadLeft(5, ' '));
            //str.Append(" %");
            //writer.Write(str.ToString());

            var bars = (int)(d / 100 * totalBars);
            writer.Write(new string('\b', 30));
            writer.Write("|");
            writer.Write(new string('-', bars), WriteStyle.Highlight1);
            writer.Write(new string(' ', totalBars - bars) + "| ");
            writer.Write($"{d:N1}".PadLeft(5, 'x'), WriteStyle.Highlight3);
            writer.Write(" %");
        };

        return new Progress<double>(act.Debounce());
    }

    private static Action<T> Debounce<T>(this Action<T> func, int milliseconds = 200)
    {
        CancellationTokenSource? cancelTokenSource = null;
        return arg =>
        {
            cancelTokenSource?.Cancel();
            cancelTokenSource = new CancellationTokenSource();
            Task.Delay(milliseconds, cancelTokenSource.Token)
                .ContinueWith(t =>
                {
                    if (t.IsCompletedSuccessfully)
                    {
                        func(arg);
                    }
                }, TaskScheduler.Default);
        };
    }
}
