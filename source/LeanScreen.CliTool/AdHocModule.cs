// <copyright file="AdHocModule.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace LeanScreen.CliTool;

using Comanche;
using LeanScreen.Extensions;

/// <summary>
/// The ad-hoc module.
/// </summary>
public class AdHocModule(IConsole console) : IModule
{
    /// <summary>
    /// Finds uncapped video in a store and caps it.
    /// </summary>
    /// <param name="source">The source directory.</param>
    /// <param name="target">Optional target directory. If not provided, caps are inlined.</param>
    /// <param name="recurse">Whether to look in source sub-directories.</param>
    /// <returns>The number of new caps.</returns>
    public async Task<int> Cap(
        [Alias("s")] string source,
        [Alias("t")] string? target = null,
        [Alias("r")] bool recurse = false)
    {
        var di = new DirectoryInfo(source);
        var targetInfo = target == null ? null : new DirectoryInfo(target);
        var result = await di.ApplyCaps(targetInfo, recurse, console.ProgressHandler());
        await Task.Delay(1000);
        console.WriteLine();
        return result;
    }
}
