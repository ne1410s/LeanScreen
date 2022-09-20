using System.Reflection;
using System.Text.RegularExpressions;
using Comanche;
using Comanche.Services;

namespace AvCtl.Tests;

public class SessionTests
{
    [Fact]
    public void Route_WithParams_ReturnsExpected()
    {
        // Arrange
        var input = "demo add --n1 1 --n2 1";

        // Act
        var result = Route(input);

        // Assert
        result.Should().Be(2);
    }

    private static object? Route(string consoleInput, IOutputWriter? writer = null)
    {
        Environment.ExitCode = 0;
        return Session.Route(
            Regex.Split(consoleInput, "\\s+"),
            Assembly.GetAssembly(typeof(DemoModule)),
            writer);
    }
}
