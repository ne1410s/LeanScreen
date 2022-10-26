// <copyright file="CryptModuleTests.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

using Comanche.Services;

namespace AvCtl.Tests;

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
        CryptModule.EncryptMedia(root, keyCsv, mockWriter.Object);

        // Assert
        mockWriter.Verify(m => m.WriteLine("Encryption: Start - Files: 3", false), Times.Once());
        mockWriter.Verify(m => m.WriteLine("Done: 33.33%", false), Times.Once());
        mockWriter.Verify(m => m.WriteLine("Done: 66.67%", false), Times.Once());
        mockWriter.Verify(m => m.WriteLine("Done: 100.00%", false), Times.Once());
        mockWriter.Verify(m => m.WriteLine("Encryption: End", false), Times.Once());
        mockWriter.Verify(
            m => m.WriteLine(It.Is<string>(s => s.StartsWith(" - Not secured: ")), false),
            Times.Exactly(3));
    }
}
