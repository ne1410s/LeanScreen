// <copyright file="TestHelper.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace AvCtl.Tests;

using System.Reflection;
using System.Text.RegularExpressions;
using Comanche;
using Comanche.Services;

public static class TestHelper
{
    public static object? Route(string consoleInput, IOutputWriter? writer = null)
    {
        Environment.ExitCode = 0;
        return Discover.Go(
            true,
            Assembly.GetAssembly(typeof(InfoModule)),
            Regex.Split(consoleInput, "\\s+"),
            writer);
    }

    public static string CloneSamples()
    {
        var targetRoot = Guid.NewGuid().ToString();
        var di = new DirectoryInfo("Samples");
        foreach (var file in di.EnumerateFiles("*sample.*v*", SearchOption.AllDirectories))
        {
            var target = file!.DirectoryName!.Replace("Samples", targetRoot, StringComparison.OrdinalIgnoreCase);
            Directory.CreateDirectory(target);
            File.Copy(file.FullName, Path.Combine(target, file.Name));
        }

        return targetRoot;
    }
}
