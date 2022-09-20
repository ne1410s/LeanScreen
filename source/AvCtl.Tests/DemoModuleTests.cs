namespace AvCtl.Tests
{
    /// <summary>
    /// Tests for the <see cref="DemoModule"/>.
    /// </summary>
    public class DemoModuleTests
    {
        [Fact]
        public void Add_WithNumbers_IsExpected()
        {
            // Arrange
            const int n1 = 1;
            const int n2 = 2;

            // Act
            var result = DemoModule.Add(n1, n2);

            // Assert
            result.Should().Be(3);
        }
    }
}