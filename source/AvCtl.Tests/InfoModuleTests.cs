// <copyright file="InfoModuleTests.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

using System.Reflection;
using System.Text.RegularExpressions;
using Comanche;
using Comanche.Services;

namespace AvCtl.Tests;

/// <summary>
/// Tests for the <see cref="InfoModule"/>.
/// </summary>
public class InfoModuleTests
{
    [Theory]
    [InlineData("4a3a54004ec9482cb7225c2574b0f889291e8270b1c4d61dbc1ab8d9fef4c9e0.mp4", "9,0,2,1,0")]
    [InlineData("sample.avi")]
    [InlineData("sample.flv")]
    [InlineData("sample.mkv")]
    [InlineData("sample.mp4")]
    public void GetBasicInfo_SecureFile_ReturnsMessage(string fileName, string? keyCsv = null)
    {
        // Arrange
        var source = Path.Combine("Samples", fileName);
        var keyArg = keyCsv == null ? null : $"-k {keyCsv}";
        var expected = File.ReadAllText(Path.Combine("Samples", "Info", fileName + ".json"));

        // Act
        var result = Route($"info basic -s {source} {keyArg}");

        // Assert
        result.Should().Be(expected);
    }

    private static object? Route(string consoleInput, IOutputWriter? writer = null)
    {
        Environment.ExitCode = 0;
        return Session.Route(
            Regex.Split(consoleInput, "\\s+"),
            Assembly.GetAssembly(typeof(InfoModule)),
            writer);
    }
}
