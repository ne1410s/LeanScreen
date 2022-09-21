namespace Av.Tests;

/// <summary>
/// Tests for the <see cref="ThumbingExtensions"/>.
/// </summary>
public class ThumbingExtensionsTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public void Distribute_WithCountN_CalledNTimes(int expected)
    {
        // Arrange
        var actual = 0;
        var duration = TimeSpan.FromSeconds(1);

        // Act
        duration.Distribute(expected, (_, _) => actual++);

        // Assert
        actual.Should().Be(expected);
    }

    [Fact]
    public void Distribute_WithValues_SpacedBetween()
    {
        // Arrange
        var callTimes = new List<TimeSpan>();
        var duration = TimeSpan.FromSeconds(19);
        var expected = Enumerable.Range(0, 20).Select(n => TimeSpan.FromSeconds(n));

        // Act
        duration.Distribute(20, (t, _) => callTimes.Add(t));

        // Assert
        callTimes.Should().BeEquivalentTo(expected);
    }
}
