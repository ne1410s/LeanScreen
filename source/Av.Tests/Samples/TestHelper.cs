// <copyright file="TestHelper.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace Av.Tests.Samples;

public static class TestHelper
{
    public static DirectoryInfo CopySamples() => CopyAll(new("Samples"));

    private static DirectoryInfo CopyAll(DirectoryInfo source, DirectoryInfo? target = null)
    {
        target ??= new($"{source.Name}_{Guid.NewGuid()}");
        target.Create();

        foreach (var di in source.GetDirectories())
        {
            CopyAll(di, target.CreateSubdirectory(di.Name));
        }

        foreach (var fi in source.GetFiles())
        {
            fi.CopyTo(Path.Combine(target.FullName, fi.Name));
        }

        return target;
    }
}
