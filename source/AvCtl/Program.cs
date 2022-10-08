// <copyright file="Program.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

#if DEBUG
_ = Comanche.Session.Route(new[]
{
    "snap",
    "evenly",
    "-s C:\\temp\\media\\sample.mp4",
});
#else
_ = Comanche.Session.Route();
#endif

/// <summary>
/// The program.
/// </summary>
[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
#pragma warning disable CA1050
public partial class Program { }
#pragma warning restore CA1050