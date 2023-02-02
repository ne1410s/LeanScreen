// <copyright file="InfoModuleTests.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace AvCtl.Tests;

using System.Collections.ObjectModel;
using Comanche.Services;
using Crypt.IO;

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
        //var file = new FileInfo(Path.Combine("Samples", "4a3a54004ec9482cb7225c2574b0f889291e8270b1c4d61dbc1ab8d9fef4c9e0.mp4"));
        //var key = new byte[] { 26, 13, 129, 173, 11, 210, 216, 47, 15, 72, 217, 141, 124, 3, 238, 238, 97, 90, 73, 255 };
        //var fs = File.OpenWrite("Samples/Info/dec.mp4");
        //file.DecryptTo(fs, key);

        // Arrange
        var source = Path.Combine("Samples", fileName);
        var expected = File.ReadAllText(Path.Combine("Samples", "Info", fileName + ".json"));
        var mockWriter = new Mock<IOutputWriter>();
        mockWriter
            .Setup(m => m.CaptureStrings(It.IsAny<string>()))
            .Returns(new Collection<string>(new[] { "123", "abc" }));

        // Act
        var result = TestHelper.Route($"info basic -s {source} -ks Samples -kr xyz", mockWriter.Object);

        // Assert
        result.Should().Be(expected);
    }
}
