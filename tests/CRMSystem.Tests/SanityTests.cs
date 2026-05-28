using FluentAssertions;
using Xunit;

namespace CRMSystem.Tests;

public class SanityTests
{
    [Fact]
    public void Math_Should_Still_Work()
    {
        // Arrange
        var a = 2;
        var b = 3;

        // Act
        var result = a + b;

        // Assert
        result.Should().Be(5);
    }
}