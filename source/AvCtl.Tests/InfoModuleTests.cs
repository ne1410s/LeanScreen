// <copyright file="InfoModuleTests.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace AvCtl.Tests;

using Comanche.Services;

/// <summary>
/// Tests for the <see cref="InfoModule"/>.
/// </summary>
public class InfoModuleTests
{
    [Fact]
    public void GetBasicInfo_NoWriter_ThrowsException()
    {
        // Arrange
        IOutputWriter writer = null!;

        // Act
        var act = () => InfoModule.GetBasicInfo(writer, null!);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName(nameof(writer));
    }

    [Fact]
    public void GetBasicInfo_WithWriter_ThrowsException()
    {
        // Arrange
        var mockWriter = new Mock<IOutputWriter>();

        // Act
        var act = () => InfoModule.GetBasicInfo(mockWriter.Object, "blah");

        // Assert
        act.Should().Throw<NullReferenceException>();
        mockWriter.Verify(m => m.WriteLine(It.Is<string>(s => s.StartsWith("Keys: 0, Check: ")), false));
    }

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

        // Act
        var result = TestHelper.Route($"info basic -s {source} -ks Samples -kr xyz");

        // Assert
        result.Should().Be(expected);
    }
}
