// <copyright file="InfoModuleTests.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

using System.Collections.ObjectModel;
using Comanche.Services;

namespace AvCtl.Tests;

/// <summary>
/// Tests for the <see cref="InfoModule"/>.
/// </summary>
public class InfoModuleTests
{
    [Theory]
    [InlineData("4a3a54004ec9482cb7225c2574b0f889291e8270b1c4d61dbc1ab8d9fef4c9e0.mp4")]
    [InlineData("sample.avi")]
    [InlineData("sample.flv")]
    [InlineData("sample.mkv")]
    [InlineData("sample.mp4")]
    public void GetBasicInfo_SecureFile_ReturnsMessage(string fileName)
    {
        // Arrange
        var source = Path.Combine("Samples", fileName);
        var expected = File.ReadAllText(Path.Combine("Samples", "Info", fileName + ".json"));
        var mockWriter = new Mock<IOutputWriter>();
        mockWriter
            .Setup(m => m.CaptureStrings(It.IsAny<string>()))
            .Returns(new Collection<string>(new[] { "123", "abc" }));

        // Act
        var result = TestHelper.Route($"info basic -s {source} -ks . -kr .", mockWriter.Object);

        // Assert
        result.Should().Be(expected);
    }
}
