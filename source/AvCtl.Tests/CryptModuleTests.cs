﻿// <copyright file="CryptModuleTests.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace AvCtl.Tests;

using Av;
using Av.Models;
using Comanche.Services;

/// <summary>
/// Tests for the <see cref="CryptModule"/>.
/// </summary>
public class CryptModuleTests
{
    [Fact]
    public void EncryptMedia_WithFiles_ReportsProgress()
    {
        // Arrange
        const string keyCsv = "9,0,2,1,0";
        var root = TestHelper.CloneSamples();
        var mockWriter = new Mock<IOutputWriter>();

        // Act
        CryptModule.EncryptMedia(root, keyCsv, 2, mockWriter.Object);

        // Assert
        mockWriter.Verify(m => m.WriteLine("Encryption: Start - Files: 3", false), Times.Once());
        mockWriter.Verify(m => m.WriteLine("Done: 33.33%", false), Times.Once());
        mockWriter.Verify(m => m.WriteLine("Done: 66.67%", false), Times.Once());
        mockWriter.Verify(m => m.WriteLine("Done: 100.00%", false), Times.Once());
        mockWriter.Verify(m => m.WriteLine("Encryption: End", false), Times.Once());
        mockWriter.Verify(
            m => m.WriteLine(It.Is<string>(s => s.StartsWith(" - Not secured: ")), false),
            Times.Exactly(3));

        var remains = new DirectoryInfo(root).EnumerateMedia(MediaTypes.AnyMedia, false);
        remains.Should().BeEmpty();
    }

    [Fact]
    public void EncryptMedia_WithNoWriter_WritesToConsole()
    {
        // Arrange
        const string keyCsv = "9,0,2,1,0";
        var root = TestHelper.CloneSamples();
        var writer = new StringWriter();
        Console.SetOut(writer);

        // Act
        CryptModule.EncryptMedia(root, keyCsv);

        // Assert
        writer.ToString().Should().Contain("Encryption: Start");
    }

    [Fact]
    public void EncryptMedia_WithDepth_MovesFile()
    {
        // Arrange
        const string keyCsv = "9,0,2,1,0";
        const int groupLength = 3;
        const string fileName = "44e339204806870505a2a448115b2e554080cee37ddfb46949e47f1c586b011f.mkv";
        var root = TestHelper.CloneSamples();
        var expectedLocation = Path.Combine(root, fileName[..groupLength], fileName);
        var expectedRemoval = Path.Combine(root, fileName);
        var writer = new StringWriter();
        Console.SetOut(writer);

        // Act
        CryptModule.EncryptMedia(root, keyCsv, 3);
        var creationCheck = File.Exists(expectedLocation);
        var removalCheck = !File.Exists(expectedRemoval);

        // Assert
        creationCheck.Should().BeTrue();
        removalCheck.Should().BeTrue();
    }
}
