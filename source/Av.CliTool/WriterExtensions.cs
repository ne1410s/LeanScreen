// <copyright file="WriterExtensions.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace Av.CliTool;

using System.Text.RegularExpressions;
using Comanche;

/// <summary>
/// Extensions for the output writer.
/// </summary>
public static class WriterExtensions
{
    /// <summary>
    /// Prepares a key.
    /// </summary>
    /// <param name="console">The console.</param>
    /// <param name="keySource">The key source.</param>
    /// <param name="keyRegex">The key regex.</param>
    /// <returns>The key.</returns>
    public static byte[] PrepareKey(this IConsole console, string? keySource, string? keyRegex)
    {
        console = console.NotNull();
        var keyDi = keySource == null ? null : new DirectoryInfo(keySource);
        var keyReg = keyRegex == null ? null : new Regex(keyRegex);
        var key = keyDi.MakeKey(keyReg, console.CaptureStrings(mask: '*'), out var checkSum);
        console.WriteSecondary(checkSum, true);
        console.CaptureStrings("[Press RETURN to continue]", max: 1);
        console.Write(line: true);
        return key.ToArray();
    }

    /// <summary>
    /// Gets a progress writer.
    /// </summary>
    /// <param name="console">The writer.</param>
    /// <returns>Progress handler.</returns>
    public static IProgress<double> ProgressHandler(this IConsole console)
    {
        const int totalBars = 20;
        Action<double> act = d =>
        {
            var bars = (int)(d / 100 * totalBars);
            console.Write(new string('\b', 30));
            console.Write("|");
            console.WritePrimary(new string('-', bars));
            console.Write(new string(' ', totalBars - bars) + "| ");
            console.WriteTertiary($"{d:N1}".PadLeft(5, 'x'));
            console.Write(" %");
        };

        return new Progress<double>(act.Debounce());
    }

    private static Action<T> Debounce<T>(this Action<T> func, int milliseconds = 200)
    {
        CancellationTokenSource? cancelTokenSource = null;
        return arg =>
        {
            try
            {
                cancelTokenSource?.Cancel();
            }
            catch
            {
                // Just do nothing
            }

            cancelTokenSource = new CancellationTokenSource();
            Task.Delay(milliseconds, cancelTokenSource.Token)
                .ContinueWith(
                    t =>
                    {
                        if (t.IsCompletedSuccessfully)
                        {
                            func(arg);
                            cancelTokenSource.Dispose();
                        }
                    }, TaskScheduler.Default);
        };
    }
}
